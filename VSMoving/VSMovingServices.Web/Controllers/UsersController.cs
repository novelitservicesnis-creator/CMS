using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly VSMovingDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        VSMovingDbContext db,
        IPasswordHasher<User> passwordHasher,
        ILogger<UsersController> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out int id) ? id : 0;
    }

    [HttpGet("/Admin/Users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.Role == "Admin")
            .ThenByDescending(u => u.CreatedAt)
            .ToListAsync();

        var vm = new UsersListViewModel
        {
            Users = users,
            CurrentUserId = GetCurrentUserId()
        };

        return View(vm);
    }

    [HttpPost("/Admin/Users/Create")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateUserInputModel input)
    {
        var fullName = input.FullName?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();
        var role = string.Equals(input.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Agent";

        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest(new { success = false, message = "Full Name is required." });

        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out _))
            return BadRequest(new { success = false, message = "Please enter a valid email address." });

        if (string.IsNullOrWhiteSpace(input.Password) || input.Password.Length < 6)
            return BadRequest(new { success = false, message = "Password must be at least 6 characters long." });

        if (!string.Equals(input.Password, input.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new { success = false, message = "Password and Confirm Password do not match." });

        var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (emailExists)
            return BadRequest(new { success = false, message = "A user with this email address already exists." });

        try
        {
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                Role = role,
                IsActive = input.IsActive,
                CreatedAt = DateTime.Now
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"User '{user.FullName}' ({user.Role}) created successfully.",
                data = new { user.UserId, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user account for {Email}.", email);
            return StatusCode(500, new { success = false, message = "Unable to create user. Please try again." });
        }
    }

    [HttpPost("/Admin/Users/Edit")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] EditUserInputModel input)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        var fullName = input.FullName?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var phone = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();
        var role = string.Equals(input.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Agent";

        if (string.IsNullOrWhiteSpace(fullName))
            return BadRequest(new { success = false, message = "Full Name is required." });

        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out _))
            return BadRequest(new { success = false, message = "Please enter a valid email address." });

        var duplicateEmail = await _db.Users.AnyAsync(u => u.UserId != input.UserId && u.Email.ToLower() == email.ToLower());
        if (duplicateEmail)
            return BadRequest(new { success = false, message = "Another user is already using this email address." });

        var currentUserId = GetCurrentUserId();
        if (user.UserId == currentUserId)
        {
            if (!input.IsActive)
                return BadRequest(new { success = false, message = "You cannot deactivate your own active Admin account." });
            if (role != "Admin")
                return BadRequest(new { success = false, message = "You cannot remove the Admin role from your own account." });
        }

        try
        {
            user.FullName = fullName;
            user.Email = email;
            user.PhoneNumber = phone;
            user.Role = role;
            user.IsActive = input.IsActive;
            user.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"User '{user.FullName}' updated successfully.",
                data = new { user.UserId, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user #{UserId}.", input.UserId);
            return StatusCode(500, new { success = false, message = "Unable to update user. Please try again." });
        }
    }

    [HttpPost("/Admin/Users/ToggleStatus")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus([FromForm] int userId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        if (user.UserId == GetCurrentUserId())
            return BadRequest(new { success = false, message = "You cannot deactivate your own account." });

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        var stateText = user.IsActive ? "activated" : "deactivated";
        return Ok(new
        {
            success = true,
            message = $"User '{user.FullName}' has been {stateText}.",
            data = new { user.UserId, user.IsActive }
        });
    }

    [HttpPost("/Admin/Users/ResetPassword")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromForm] ResetUserPasswordInputModel input)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == input.UserId);
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        if (string.IsNullOrWhiteSpace(input.NewPassword) || input.NewPassword.Length < 6)
            return BadRequest(new { success = false, message = "New password must be at least 6 characters long." });

        if (!string.Equals(input.NewPassword, input.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new { success = false, message = "New Password and Confirm Password do not match." });

        user.PasswordHash = _passwordHasher.HashPassword(user, input.NewPassword);
        user.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = $"Password updated securely for '{user.FullName}'."
        });
    }
}

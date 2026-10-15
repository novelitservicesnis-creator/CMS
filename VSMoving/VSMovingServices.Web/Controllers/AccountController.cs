using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Controllers;

public class AccountController : Controller
{
    private readonly VSMovingDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        VSMovingDbContext db,
        IPasswordHasher<User> passwordHasher,
        ILogger<AccountController> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet("/Login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/Login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("Login failed: unknown email {Email}.", normalizedEmail);
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View(model);
        }

        PasswordVerificationResult verifyResult;
        try
        {
            verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        }
        catch (FormatException)
        {
            verifyResult = PasswordVerificationResult.Failed;
        }

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed: invalid password for user {Email}.", normalizedEmail);
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View(model);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login blocked: inactive user account {Email}.", normalizedEmail);
            ModelState.AddModelError(string.Empty, "Your account is currently inactive. Please contact an administrator.");
            return View(model);
        }

        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
            user.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("PhoneNumber", user.PhoneNumber ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) &&
            !model.ReturnUrl.StartsWith("/Login", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Dashboard", "Admin");
    }

    [HttpPost("/Logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("/Logout")]
    [Authorize]
    public async Task<IActionResult> LogoutGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("/AccessDenied")]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        return View();
    }
}

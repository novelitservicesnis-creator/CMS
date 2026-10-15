using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Services;

namespace VSMovingServices.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SystemSettingsController : Controller
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        IEmailService emailService,
        ILogger<SystemSettingsController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("/Admin/SystemSettings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var vm = await _emailService.GetEmailSettingsViewModelAsync();
        return View(vm);
    }

    [HttpPost("/Admin/SystemSettings/SaveEmailSettings")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEmailSettings([FromForm] SaveEmailSettingsInputModel input)
    {
        try
        {
            var result = await _emailService.SaveEmailSettingsAsync(input);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    data = result.ViewModel
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.ViewModel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving SMTP settings.");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while saving email settings."
            });
        }
    }

    [HttpPost("/Admin/SystemSettings/TestConnection")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnection([FromForm] SaveEmailSettingsInputModel input)
    {
        try
        {
            var result = await _emailService.TestConnectionAsync(input);
            if (!result.Success)
            {
                return Ok(new
                {
                    success = false,
                    message = result.Message,
                    data = result.ViewModel
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.ViewModel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during SMTP test connection.");
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to complete the SMTP connection test."
            });
        }
    }

    [HttpPost("/Admin/SystemSettings/SaveWebsiteSettings")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWebsiteSettings([FromForm] SaveWebsiteSettingsInputModel input)
    {
        try
        {
            var result = await _emailService.SaveWebsiteSettingsAsync(input);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    data = result.ViewModel
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.ViewModel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving Website & Social settings.");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while saving website settings."
            });
        }
    }
}

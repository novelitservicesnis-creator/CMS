using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;
using VSMovingServices.Web.Services;

namespace VSMovingServices.Web.Controllers;

public class HomeController : Controller
{
    private readonly VSMovingDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ITestimonialService _testimonialService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        VSMovingDbContext db,
        IEmailService emailService,
        ITestimonialService testimonialService,
        ILogger<HomeController> logger)
    {
        _db = db;
        _emailService = emailService;
        _testimonialService = testimonialService;
        _logger = logger;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var settings = await _emailService.GetWebsiteSettingsAsync();
        settings.Testimonials = await _testimonialService.GetPublishedTestimonialsAsync();
        ViewData["WebsiteSettings"] = settings;
        return View(settings);
    }

    [HttpGet("/AboutUs")]
    public async Task<IActionResult> AboutUs()
    {
        var settings = await _emailService.GetWebsiteSettingsAsync();
        ViewData["WebsiteSettings"] = settings;
        return View(settings);
    }

    [HttpGet("/PrivacyPolicy")]
    public async Task<IActionResult> PrivacyPolicy()
    {
        var settings = await _emailService.GetWebsiteSettingsAsync();
        ViewData["WebsiteSettings"] = settings;
        return View(settings);
    }

    [HttpGet("/TermsAndConditions")]
    public async Task<IActionResult> TermsAndConditions()
    {
        var settings = await _emailService.GetWebsiteSettingsAsync();
        ViewData["WebsiteSettings"] = settings;
        return View(settings);
    }

    [HttpPost("/api/leads")]
    public async Task<IActionResult> SubmitLead([FromBody] LeadSubmitInputModel? input)
    {
        if (input == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Please fill in the required fields.",
                errors = new[] { "Invalid request payload." }
            });
        }

        // Honeypot check: silently succeed if filled by a bot
        if (!string.IsNullOrWhiteSpace(input.Website))
        {
            return Ok(new
            {
                success = true,
                message = "Thank you! Your request has been received. We will contact you shortly."
            });
        }

        var fullName = input.FullName?.Trim() ?? string.Empty;
        var phone = input.Phone?.Trim() ?? string.Empty;
        var email = input.Email?.Trim() ?? string.Empty;
        var message = string.IsNullOrWhiteSpace(input.Message) ? null : input.Message.Trim();

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add("Full Name is required.");
        else if (fullName.Length > 100)
            errors.Add("Full Name cannot exceed 100 characters.");

        var digitsCount = phone.Count(char.IsDigit);
        if (string.IsNullOrWhiteSpace(phone) || digitsCount < 7)
            errors.Add("Please enter a valid phone number.");

        if (string.IsNullOrWhiteSpace(email) || !MailAddress.TryCreate(email, out _))
            errors.Add("Please enter a valid email address.");

        if (message != null && message.Length > 1000)
            errors.Add("Message cannot exceed 1000 characters.");

        if (errors.Count > 0)
        {
            return BadRequest(new
            {
                success = false,
                message = errors[0],
                errors
            });
        }

        try
        {
            var lead = new Lead
            {
                FullName = fullName,
                PhoneNumber = phone,
                Email = email,
                Message = message,
                Source = "VS Moving Services Website",
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            // Attempt email notification after database commit (never rollback lead if SMTP fails)
            _ = await _emailService.SendLeadNotificationAsync(lead);

            return Ok(new
            {
                success = true,
                message = "Thank you! Your request has been received. We will contact you shortly.",
                data = new { leadId = lead.LeadId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save website enquiry lead.");
            return StatusCode(500, new
            {
                success = false,
                message = "Sorry, something went wrong. Please call us at 514 999 9790 or try again."
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        var settings = await _emailService.GetWebsiteSettingsAsync();
        ViewData["WebsiteSettings"] = settings;
        return View(settings);
    }
}

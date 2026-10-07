using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using AgamEstates.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly Regex PhoneValidationRegex = new(@"^[0-9+()\- \s]{7,20}$", RegexOptions.Compiled);

        private readonly ILeadRepository? _leadRepository;
        private readonly ILeadStatusRepository? _leadStatusRepository;
        private readonly ILeadCommunicationRepository? _leadCommunicationRepository;
        private readonly ISystemSettingsService? _systemSettingsService;
        private readonly IEmailSettingsService? _emailSettingsService;
        private readonly IEmailService? _emailService;

        public HomeController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<HomeController> logger,
            ILeadRepository? leadRepository = null,
            ILeadStatusRepository? leadStatusRepository = null,
            ILeadCommunicationRepository? leadCommunicationRepository = null,
            ISystemSettingsService? systemSettingsService = null,
            IEmailSettingsService? emailSettingsService = null,
            IEmailService? emailService = null,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _leadRepository = leadRepository;
            _leadStatusRepository = leadStatusRepository;
            _leadCommunicationRepository = leadCommunicationRepository;
            _systemSettingsService = systemSettingsService;
            _emailSettingsService = emailSettingsService;
            _emailService = emailService;
        }

        [HttpGet("/")]
        [HttpGet("/Home")]
        [HttpGet("/Home/Index")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Agam Estates — Sector 20, Panchkula Extension";
            ViewData["ActiveNav"] = "Home";
            return View();
        }

        [HttpGet("/Project")]
        [HttpGet("/Home/Project")]
        public IActionResult Project()
        {
            ViewData["Title"] = "Our Project — Agam Estates";
            ViewData["ActiveNav"] = "Project";
            return View();
        }

        [HttpGet("/PrivacyPolicy")]
        [HttpGet("/Home/PrivacyPolicy")]
        [HttpGet("/Privacy")]
        public IActionResult PrivacyPolicy()
        {
            ViewData["Title"] = "Privacy Policy — Agam Estates";
            ViewData["ActiveNav"] = "PrivacyPolicy";
            return View();
        }

        [HttpGet("/AboutUs")]
        [HttpGet("/Home/AboutUs")]
        [HttpGet("/About")]
        public IActionResult AboutUs()
        {
            ViewData["Title"] = "About Us — Agam Estates";
            ViewData["ActiveNav"] = "AboutUs";
            return View();
        }

        [HttpGet("/TermsAndConditions")]
        [HttpGet("/Home/TermsAndConditions")]
        [HttpGet("/Terms")]
        public IActionResult TermsAndConditions()
        {
            ViewData["Title"] = "Terms & Conditions — Agam Estates";
            ViewData["ActiveNav"] = "TermsAndConditions";
            return View();
        }

        [HttpPost("/Home/SubmitEnquiry")]
        [HttpPost("/SubmitEnquiry")]
        public async Task<IActionResult> SubmitEnquiry([FromBody] EnquiryViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name) || model.Name.Trim().Length < 2)
            {
                return Json(new { success = false, message = "Please provide your full name." });
            }

            var cleanName = Regex.Replace(model.Name.Trim(), @"[\r\n\0]+", " ");
            var cleanPhone = model.PhoneNumber?.Trim() ?? string.Empty;
            var cleanEmail = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cleanPhone) || !PhoneValidationRegex.IsMatch(cleanPhone))
            {
                return Json(new { success = false, message = "Please provide a valid phone number." });
            }

            if (string.IsNullOrWhiteSpace(cleanEmail) || !IsValidEmailAddress(cleanEmail))
            {
                return Json(new { success = false, message = "Please provide a valid email address." });
            }

            var submittedInterest = !string.IsNullOrWhiteSpace(model.PropertyInterest)
                ? Regex.Replace(model.PropertyInterest.Trim(), @"[\r\n\0]+", " ")
                : string.Empty;

            var cleanInterest = !string.IsNullOrWhiteSpace(submittedInterest)
                ? submittedInterest
                : "3 & 4 BHK Residences";

            var cleanMessage = !string.IsNullOrWhiteSpace(model.Message)
                ? model.Message.Trim()
                : null;

            try
            {
                // 1. Resolve 'New' Lead Status
                int statusId = 1;
                if (_leadStatusRepository != null)
                {
                    var statuses = await _leadStatusRepository.GetAllStatusesAsync();
                    var newStatus = statuses.FirstOrDefault(s => s.StatusName.Equals("New", StringComparison.OrdinalIgnoreCase));
                    if (newStatus != null)
                    {
                        statusId = newStatus.StatusId;
                    }
                    else if (statuses.Any())
                    {
                        statusId = statuses.First().StatusId;
                    }
                }

                // 2. Save Lead / Enquiry to Database FIRST
                int leadId = 0;
                var submittedAtIst = GetIstTimeNow();

                if (_leadRepository != null)
                {
                    leadId = await _leadRepository.CreateLeadAsync(new CreateLeadDto
                    {
                        Name = cleanName,
                        PhoneNumber = cleanPhone,
                        Email = cleanEmail,
                        PropertyInterest = cleanInterest,
                        Budget = model.Budget,
                        City = model.City?.Trim() ?? "Tricity",
                        Source = "Website Enquiry",
                        StatusId = statusId,
                        Priority = "High",
                        Notes = cleanMessage
                    });

                    if (leadId > 0 && !string.IsNullOrWhiteSpace(cleanMessage) && _leadCommunicationRepository != null)
                    {
                        await _leadCommunicationRepository.CreateCommunicationAsync(new CreateLeadCommunicationDto
                        {
                            LeadId = leadId,
                            UserId = 1, // Default to admin
                            CommunicationType = "Website Form",
                            Message = $"Initial enquiry: {cleanMessage}",
                            FollowUpDate = DateTime.Today.AddHours(18)
                        });
                    }
                }

                // 3. Send Branded Email Notification (isolated try/catch so email failure NEVER rolls back or deletes the saved lead)
                if (_emailService != null)
                {
                    try
                    {
                        var receiverEmail = _emailSettingsService != null
                            ? await _emailSettingsService.GetReceiverEmailAsync()
                            : string.Empty;

                        if (string.IsNullOrWhiteSpace(receiverEmail) && _systemSettingsService != null)
                        {
                            var siteSettings = await _systemSettingsService.GetPublicSettingsAsync();
                            receiverEmail = siteSettings?.Email?.Trim() ?? string.Empty;
                        }

                        var subject = _emailService.BuildEnquirySubject(cleanName);
                        var htmlBody = _emailService.BuildEnquiryNotificationHtml(new EnquiryEmailNotificationModel
                        {
                            LeadId = leadId,
                            FullName = cleanName,
                            PhoneNumber = cleanPhone,
                            Email = cleanEmail,
                            InterestedIn = submittedInterest,
                            Message = cleanMessage,
                            SubmittedAt = submittedAtIst
                        });

                        await _emailService.SendEmailAsync(
                            recipientEmail: receiverEmail,
                            subject: subject,
                            htmlBody: htmlBody,
                            replyToEmail: cleanEmail,
                            replyToName: cleanName);

                        _logger.LogInformation("Website enquiry notification sent successfully for LeadId {LeadId}", leadId);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Website enquiry saved but notification email failed for LeadId {LeadId}", leadId);
                    }
                }

                // 4. Return existing success response
                return Json(new
                {
                    success = true,
                    message = "Thank you! Your enquiry has been received. Our luxury property advisor will contact you shortly."
                });
            }
            catch (Exception ex)
            {
                await LogException(ex);
                return Json(new
                {
                    success = false,
                    message = "Thank you for reaching out. We received your details and our team will get in touch soon."
                });
            }
        }

        private static bool IsValidEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
            {
                return false;
            }

            if (email.IndexOfAny(new[] { '\r', '\n', '\0', ' ' }) >= 0)
            {
                return false;
            }

            var atIndex = email.IndexOf('@');
            var lastDotIndex = email.LastIndexOf('.');
            if (atIndex <= 0 || lastDotIndex <= atIndex + 1 || lastDotIndex >= email.Length - 2)
            {
                return false;
            }

            return MailAddress.TryCreate(email, out var parsed) &&
                   string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

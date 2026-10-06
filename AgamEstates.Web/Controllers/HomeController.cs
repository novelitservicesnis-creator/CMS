using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILeadRepository? _leadRepository;
        private readonly ILeadStatusRepository? _leadStatusRepository;
        private readonly ILeadCommunicationRepository? _leadCommunicationRepository;

        public HomeController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<HomeController> logger,
            ILeadRepository? leadRepository = null,
            ILeadStatusRepository? leadStatusRepository = null,
            ILeadCommunicationRepository? leadCommunicationRepository = null,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _leadRepository = leadRepository;
            _leadStatusRepository = leadStatusRepository;
            _leadCommunicationRepository = leadCommunicationRepository;
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
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Please provide your full name." });
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber) && string.IsNullOrWhiteSpace(model.Email))
            {
                return Json(new { success = false, message = "Please provide at least a phone number or email address." });
            }

            try
            {
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

                int leadId = 0;
                if (_leadRepository != null)
                {
                    leadId = await _leadRepository.CreateLeadAsync(new CreateLeadDto
                    {
                        Name = model.Name.Trim(),
                        PhoneNumber = model.PhoneNumber?.Trim(),
                        Email = model.Email?.Trim().ToLower(),
                        PropertyInterest = model.PropertyInterest?.Trim() ?? "3 & 4 BHK Residences",
                        Budget = model.Budget,
                        City = model.City?.Trim() ?? "Tricity",
                        Source = "Website Enquiry",
                        StatusId = statusId,
                        Priority = "High",
                        Notes = model.Message?.Trim()
                    });

                    if (leadId > 0 && !string.IsNullOrWhiteSpace(model.Message) && _leadCommunicationRepository != null)
                    {
                        await _leadCommunicationRepository.CreateCommunicationAsync(new CreateLeadCommunicationDto
                        {
                            LeadId = leadId,
                            UserId = 1, // Default to admin
                            CommunicationType = "Website Form",
                            Message = $"Initial enquiry: {model.Message.Trim()}",
                            FollowUpDate = DateTime.Today.AddHours(18)
                        });
                    }
                }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

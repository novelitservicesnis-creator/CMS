using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using AgamEstates.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemSettingsController : BaseController
    {
        private readonly ISystemSettingsRepository _settingsRepository;
        private readonly ISystemSettingsService _settingsService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private static readonly string[] AllowedImageMimeTypes = { "image/png", "image/jpeg", "image/webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        public SystemSettingsController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<SystemSettingsController> logger,
            ISystemSettingsRepository settingsRepository,
            ISystemSettingsService settingsService,
            IFileStorageService fileStorageService,
            IWebHostEnvironment env,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _settingsRepository = settingsRepository;
            _settingsService = settingsService;
            _fileStorageService = fileStorageService;
            _env = env;
        }

        private int? GetCurrentAdminUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int id))
            {
                return id;
            }
            return null;
        }

        [HttpGet("/Admin/SystemSettings")]
        [HttpGet("/SystemSettings")]
        public async Task<IActionResult> Index(string tab = "general")
        {
            ViewData["Title"] = "System Settings";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "System Settings", Url = null }
            };

            var settingsDto = await _settingsRepository.GetSettingsDtoAsync() ?? new SystemSettingDto();
            var announcements = await _settingsRepository.GetAllAnnouncementsAsync();
            var publicSettings = await _settingsRepository.GetPublicSiteSettingsAsync();

            var vm = new SystemSettingsPageViewModel
            {
                Settings = settingsDto,
                Announcements = announcements,
                GroupedBusinessHours = publicSettings?.AllGroupedBusinessHours ?? new(),
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "general" : tab.ToLowerInvariant()
            };

            return View(vm);
        }

        [HttpPost("/Admin/SystemSettings/UpdateGeneral")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGeneral(UpdateGeneralSettingsInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                return RedirectToAction(nameof(Index), new { tab = "general" });
            }

            string? logoPath = null;
            string? previousLogoPath = null;

            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                // Validate size
                if (model.LogoFile.Length > MaxImageSizeBytes)
                {
                    TempData["ErrorMessage"] = "Logo file exceeds the maximum allowed size of 5 MB.";
                    return RedirectToAction(nameof(Index), new { tab = "general" });
                }

                // Validate extension
                var ext = Path.GetExtension(model.LogoFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    TempData["ErrorMessage"] = "Invalid image format. Allowed formats: PNG, JPG, JPEG, WEBP.";
                    return RedirectToAction(nameof(Index), new { tab = "general" });
                }

                // Validate mime type
                if (!AllowedImageMimeTypes.Contains(model.LogoFile.ContentType.ToLowerInvariant()))
                {
                    TempData["ErrorMessage"] = "Invalid image content type.";
                    return RedirectToAction(nameof(Index), new { tab = "general" });
                }

                // Capture current logo path BEFORE updating database
                var currentSettings = await _settingsRepository.GetSettingsDtoAsync();
                previousLogoPath = currentSettings?.LogoPath;

                try
                {
                    logoPath = await _fileStorageService.SaveSystemLogoAsync(model.LogoFile, ext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save uploaded system logo file.");
                    TempData["ErrorMessage"] = "Unable to save the uploaded logo file. Please verify server storage permissions.";
                    return RedirectToAction(nameof(Index), new { tab = "general" });
                }
            }

            var adminId = GetCurrentAdminUserId();
            bool success = false;
            try
            {
                success = await _settingsRepository.UpdateGeneralSettingsAsync(model.CompanyName, logoPath, adminId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database update failed during UpdateGeneral.");
                success = false;
            }

            if (success)
            {
                // Only after DB update succeeds, safely delete the previous uploaded logo if it was replaced
                if (!string.IsNullOrWhiteSpace(logoPath) &&
                    !string.IsNullOrWhiteSpace(previousLogoPath) &&
                    !string.Equals(previousLogoPath, logoPath, StringComparison.OrdinalIgnoreCase))
                {
                    _fileStorageService.TryDeleteManagedSystemFile(previousLogoPath);
                }

                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Website identity updated successfully.";
            }
            else
            {
                // DB update failed: delete newly uploaded file so no orphan file remains, keeping old logo untouched
                if (!string.IsNullOrWhiteSpace(logoPath))
                {
                    _fileStorageService.TryDeleteManagedSystemFile(logoPath);
                }

                TempData["ErrorMessage"] = "Unable to update website identity. Please try again.";
            }

            return RedirectToAction(nameof(Index), new { tab = "general" });
        }

        [HttpPost("/Admin/SystemSettings/UpdateContactInfo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContactInfo(UpdateContactInfoInputModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check the contact information entered.";
                return RedirectToAction(nameof(Index), new { tab = "contact" });
            }

            var adminId = GetCurrentAdminUserId();
            var success = await _settingsRepository.UpdateContactInfoAsync(
                model.AddressLine1,
                model.AddressLine2,
                model.City,
                model.State,
                model.PostalCode,
                model.Country,
                model.Email,
                adminId);

            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Contact information updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to update contact information.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/AddPhoneNumber")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(ContactNumberInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                TempData["ErrorMessage"] = "Phone number is required.";
                return RedirectToAction(nameof(Index), new { tab = "contact" });
            }

            try
            {
                await _settingsRepository.AddContactNumberAsync(
                    model.Label,
                    model.PhoneNumber,
                    model.IsPrimary,
                    model.IsActive,
                    model.SortOrder);

                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Phone number added successfully.";
            }
            catch (Exception ex)
            {
                await LogException(ex);
                TempData["ErrorMessage"] = "Failed to add phone number. Please try again.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/UpdatePhoneNumber")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhoneNumber(ContactNumberInputModel model)
        {
            if (!model.ContactNumberId.HasValue || string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                TempData["ErrorMessage"] = "Invalid phone number details.";
                return RedirectToAction(nameof(Index), new { tab = "contact" });
            }

            try
            {
                var success = await _settingsRepository.UpdateContactNumberAsync(
                    model.ContactNumberId.Value,
                    model.Label,
                    model.PhoneNumber,
                    model.IsPrimary,
                    model.IsActive,
                    model.SortOrder);

                if (success)
                {
                    _settingsService.InvalidateCache();
                    TempData["SuccessMessage"] = "Phone number updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Phone number not found.";
                }
            }
            catch (Exception ex)
            {
                await LogException(ex);
                TempData["ErrorMessage"] = "Failed to update phone number.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/DeletePhoneNumber")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoneNumber(int id)
        {
            var success = await _settingsRepository.DeleteContactNumberAsync(id);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Phone number removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to delete phone number.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/SetPrimaryPhoneNumber")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryPhoneNumber(int id)
        {
            var success = await _settingsRepository.SetPrimaryContactNumberAsync(id);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Primary phone number updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to update primary phone number.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/TogglePhoneNumberStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePhoneNumberStatus(int id)
        {
            var success = await _settingsRepository.ToggleContactNumberStatusAsync(id);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Phone number status updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to toggle phone number status.";
            }

            return RedirectToAction(nameof(Index), new { tab = "contact" });
        }

        [HttpPost("/Admin/SystemSettings/UpdateBusinessHours")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBusinessHours(UpdateBusinessHoursInputModel model)
        {
            if (model == null || model.Hours == null || model.Hours.Count == 0)
            {
                TempData["ErrorMessage"] = "No business hours data submitted.";
                return RedirectToAction(nameof(Index), new { tab = "hours" });
            }

            var dtoList = new List<SystemBusinessHourDto>();
            foreach (var item in model.Hours)
            {
                TimeSpan? openTime = null;
                TimeSpan? closeTime = null;

                if (item.IsOpen)
                {
                    if (string.IsNullOrWhiteSpace(item.OpenTime) || string.IsNullOrWhiteSpace(item.CloseTime))
                    {
                        TempData["ErrorMessage"] = $"Please specify both Open and Close times for Day {item.DayOfWeek}.";
                        return RedirectToAction(nameof(Index), new { tab = "hours" });
                    }

                    if (TryParseTime(item.OpenTime, out var ot) && TryParseTime(item.CloseTime, out var ct))
                    {
                        if (ot >= ct)
                        {
                            TempData["ErrorMessage"] = $"Close time must be later than open time for Day {item.DayOfWeek}.";
                            return RedirectToAction(nameof(Index), new { tab = "hours" });
                        }
                        openTime = ot;
                        closeTime = ct;
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Invalid time format for Day {item.DayOfWeek}.";
                        return RedirectToAction(nameof(Index), new { tab = "hours" });
                    }
                }

                dtoList.Add(new SystemBusinessHourDto
                {
                    DayOfWeek = item.DayOfWeek,
                    IsOpen = item.IsOpen,
                    OpenTime = openTime,
                    CloseTime = closeTime
                });
            }

            var success = await _settingsRepository.UpdateBusinessHoursAsync(dtoList);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Business hours updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to update business hours.";
            }

            return RedirectToAction(nameof(Index), new { tab = "hours" });
        }

        [HttpPost("/Admin/SystemSettings/SaveAnnouncement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAnnouncement(AnnouncementInputModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Message))
            {
                TempData["ErrorMessage"] = "Announcement message is required.";
                return RedirectToAction(nameof(Index), new { tab = "announcements" });
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate.Value < model.StartDate.Value)
            {
                TempData["ErrorMessage"] = "End date must be on or after start date.";
                return RedirectToAction(nameof(Index), new { tab = "announcements" });
            }

            var adminId = GetCurrentAdminUserId();

            try
            {
                if (model.AnnouncementId.HasValue && model.AnnouncementId.Value > 0)
                {
                    var success = await _settingsRepository.UpdateAnnouncementAsync(
                        model.AnnouncementId.Value,
                        model.Title,
                        model.Message,
                        model.LinkText,
                        model.LinkUrl,
                        model.StartDate,
                        model.EndDate,
                        model.IsActive,
                        adminId);

                    if (success)
                    {
                        _settingsService.InvalidateCache();
                        TempData["SuccessMessage"] = "Announcement updated successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Announcement not found.";
                    }
                }
                else
                {
                    await _settingsRepository.CreateAnnouncementAsync(
                        model.Title,
                        model.Message,
                        model.LinkText,
                        model.LinkUrl,
                        model.StartDate,
                        model.EndDate,
                        model.IsActive,
                        adminId);

                    _settingsService.InvalidateCache();
                    TempData["SuccessMessage"] = "Announcement created successfully.";
                }
            }
            catch (Exception ex)
            {
                await LogException(ex);
                TempData["ErrorMessage"] = "Failed to save announcement.";
            }

            return RedirectToAction(nameof(Index), new { tab = "announcements" });
        }

        [HttpPost("/Admin/SystemSettings/ActivateAnnouncement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAnnouncement(int id)
        {
            var adminId = GetCurrentAdminUserId();
            var success = await _settingsRepository.ActivateAnnouncementAsync(id, adminId);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Announcement activated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to activate announcement.";
            }

            return RedirectToAction(nameof(Index), new { tab = "announcements" });
        }

        [HttpPost("/Admin/SystemSettings/DeactivateAnnouncement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAnnouncement(int id)
        {
            var adminId = GetCurrentAdminUserId();
            var success = await _settingsRepository.DeactivateAnnouncementAsync(id, adminId);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Announcement deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to deactivate announcement.";
            }

            return RedirectToAction(nameof(Index), new { tab = "announcements" });
        }

        [HttpPost("/Admin/SystemSettings/DeleteAnnouncement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var success = await _settingsRepository.DeleteAnnouncementAsync(id);
            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Announcement deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to delete announcement.";
            }

            return RedirectToAction(nameof(Index), new { tab = "announcements" });
        }

        [HttpPost("/Admin/SystemSettings/UpdateSocialLinks")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSocialLinks(UpdateSocialLinksInputModel model)
        {
            var adminId = GetCurrentAdminUserId();
            var success = await _settingsRepository.UpdateSocialLinksAsync(
                model.FacebookUrl,
                model.InstagramUrl,
                model.YouTubeUrl,
                adminId);

            if (success)
            {
                _settingsService.InvalidateCache();
                TempData["SuccessMessage"] = "Social links updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to update social links.";
            }

            return RedirectToAction(nameof(Index), new { tab = "social" });
        }

        private static bool TryParseTime(string? str, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(str)) return false;

            str = str.Trim();
            if (TimeSpan.TryParse(str, out time)) return true;

            if (DateTime.TryParseExact(str, new[] { "h:mm tt", "hh:mm tt", "H:mm", "HH:mm" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                time = dt.TimeOfDay;
                return true;
            }

            return false;
        }
    }
}

using AgamEstates.Repository.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgamEstates.Web.Models
{
    public class SystemSettingsPageViewModel
    {
        public SystemSettingDto Settings { get; set; } = new();
        public List<SystemAnnouncementDto> Announcements { get; set; } = new();
        public List<GroupedBusinessHourItem> GroupedBusinessHours { get; set; } = new();
        public string ActiveTab { get; set; } = "general";
    }

    public class UpdateGeneralSettingsInputModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(150, ErrorMessage = "Company name cannot exceed 150 characters")]
        public string CompanyName { get; set; } = string.Empty;

        public IFormFile? LogoFile { get; set; }
    }

    public class UpdateContactInfoInputModel
    {
        [StringLength(250)]
        public string? AddressLine1 { get; set; }

        [StringLength(250)]
        public string? AddressLine2 { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(200)]
        public string? Email { get; set; }
    }

    public class ContactNumberInputModel
    {
        public int? ContactNumberId { get; set; }

        [StringLength(80)]
        public string? Label { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class BusinessHoursUpdateItem
    {
        public byte DayOfWeek { get; set; }
        public bool IsOpen { get; set; }
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
    }

    public class UpdateBusinessHoursInputModel
    {
        public List<BusinessHoursUpdateItem> Hours { get; set; } = new();
    }

    public class AnnouncementInputModel
    {
        public int? AnnouncementId { get; set; }

        [StringLength(150)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Announcement message is required")]
        [StringLength(500, ErrorMessage = "Announcement message cannot exceed 500 characters")]
        public string Message { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LinkText { get; set; }

        [StringLength(500)]
        public string? LinkUrl { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateSocialLinksInputModel
    {
        [StringLength(500)]
        public string? FacebookUrl { get; set; }

        [StringLength(500)]
        public string? InstagramUrl { get; set; }

        [StringLength(500)]
        public string? YouTubeUrl { get; set; }
    }
}

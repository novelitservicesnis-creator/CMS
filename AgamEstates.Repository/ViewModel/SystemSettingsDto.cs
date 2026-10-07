using System;
using System.Collections.Generic;

namespace AgamEstates.Repository.ViewModel
{
    public class SystemSettingDto
    {
        public int SettingId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? LogoPath { get; set; }
        public string? Email { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public List<SystemContactNumberDto> ContactNumbers { get; set; } = new();
        public List<SystemBusinessHourDto> BusinessHours { get; set; } = new();
    }

    public class SystemContactNumberDto
    {
        public int ContactNumberId { get; set; }
        public int SettingId { get; set; }
        public string? Label { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string NumberClean => System.Text.RegularExpressions.Regex.Replace(PhoneNumber ?? "", @"[^\d+]", "");
    }

    public class SystemBusinessHourDto
    {
        public int BusinessHourId { get; set; }
        public int SettingId { get; set; }
        public byte DayOfWeek { get; set; } // 1=Mon, ..., 7=Sun
        public string DayName { get; set; } = string.Empty;
        public bool IsOpen { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public string OpenTimeString { get; set; } = string.Empty;
        public string CloseTimeString { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class SystemAnnouncementDto
    {
        public int AnnouncementId { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? LinkText { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public string StatusBadge
        {
            get
            {
                var now = DateTime.UtcNow;
                if (!IsActive) return "INACTIVE";
                if (EndDate.HasValue && now > EndDate.Value) return "EXPIRED";
                if (StartDate.HasValue && now < StartDate.Value) return "SCHEDULED";
                return "ACTIVE";
            }
        }

        public bool IsCurrentlyDisplayable
        {
            get
            {
                var now = DateTime.UtcNow;
                if (!IsActive) return false;
                if (StartDate.HasValue && now < StartDate.Value) return false;
                if (EndDate.HasValue && now > EndDate.Value) return false;
                return true;
            }
        }
    }

    public class PublicSiteSettingsViewModel
    {
        public string CompanyName { get; set; } = "Agam Estates";
        public string CompanyNameFirst
        {
            get
            {
                var parts = (CompanyName ?? "Agam Estates").Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : "Agam";
            }
        }
        public string CompanyNameSecond
        {
            get
            {
                var parts = (CompanyName ?? "Agam Estates").Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : "Estates";
            }
        }
        public string LogoPath { get; set; } = "/images/logo.png";
        public string? Email { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public string FormattedAddress
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(AddressLine1)) parts.Add(AddressLine1.Trim());
                if (!string.IsNullOrWhiteSpace(AddressLine2)) parts.Add(AddressLine2.Trim());
                if (!string.IsNullOrWhiteSpace(City)) parts.Add(City.Trim());
                if (!string.IsNullOrWhiteSpace(State)) parts.Add(State.Trim());
                return parts.Count > 0 ? string.Join(", ", parts) : "Sector 20, Panchkula Extension, Haryana";
            }
        }

        public string LocationShort
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AddressLine2)) return AddressLine2.Trim();
                if (!string.IsNullOrWhiteSpace(AddressLine1)) return AddressLine1.Trim();
                return "Tdi South Extension II";
            }
        }

        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YouTubeUrl { get; set; }

        public SystemContactNumberDto? PrimaryPhone { get; set; }
        public List<SystemContactNumberDto> ActivePhones { get; set; } = new();
        public List<SystemBusinessHourDto> BusinessHours { get; set; } = new();
        public SystemAnnouncementDto? ActiveAnnouncement { get; set; }

        public string BusinessHoursFormatted { get; set; } = string.Empty;
        public List<GroupedBusinessHourItem> GroupedBusinessHours { get; set; } = new();
        public List<GroupedBusinessHourItem> AllGroupedBusinessHours { get; set; } = new();
    }

    public class GroupedBusinessHourItem
    {
        public string Days { get; set; } = string.Empty;       // e.g. "Monday – Friday", "Saturday", "Sunday", "Monday – Sunday"
        public string Hours { get; set; } = string.Empty;      // e.g. "9:00 AM – 7:00 PM", "Closed"
        public bool IsClosed { get; set; }
        public bool IsOpen => !IsClosed;
    }

    public class SystemEmailSettingDto
    {
        public int EmailSettingId { get; set; }
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public bool HasSavedPassword { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; } = "Agam Estates";
        public string ReceiverEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime? LastTestedAt { get; set; }
        public bool? LastTestSucceeded { get; set; }
        public string? LastTestMessage { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public string ConnectionStatusLabel
        {
            get
            {
                if (!LastTestedAt.HasValue || !LastTestSucceeded.HasValue)
                    return "Not tested";
                return LastTestSucceeded.Value ? "Valid" : "Invalid";
            }
        }
    }
}

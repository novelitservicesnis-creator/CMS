namespace AgamEstates.Core.Models
{
    using System;
    using System.Collections.Generic;

    public partial class SystemSetting
    {
        public SystemSetting()
        {
            ContactNumbers = new HashSet<SystemContactNumber>();
            BusinessHours = new HashSet<SystemBusinessHour>();
        }

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
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<SystemContactNumber> ContactNumbers { get; set; }
        public virtual ICollection<SystemBusinessHour> BusinessHours { get; set; }
    }
}

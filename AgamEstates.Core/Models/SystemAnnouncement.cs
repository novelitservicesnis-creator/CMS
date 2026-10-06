namespace AgamEstates.Core.Models
{
    using System;

    public partial class SystemAnnouncement
    {
        public int AnnouncementId { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? LinkText { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}

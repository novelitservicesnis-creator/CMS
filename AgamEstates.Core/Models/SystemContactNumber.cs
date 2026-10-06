namespace AgamEstates.Core.Models
{
    public partial class SystemContactNumber
    {
        public int ContactNumberId { get; set; }
        public int SettingId { get; set; }
        public string? Label { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        // Navigation property
        public virtual SystemSetting? Setting { get; set; }
    }
}

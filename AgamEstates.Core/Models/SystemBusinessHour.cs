namespace AgamEstates.Core.Models
{
    using System;

    public partial class SystemBusinessHour
    {
        public int BusinessHourId { get; set; }
        public int SettingId { get; set; }
        public byte DayOfWeek { get; set; } // 1 = Monday, 2 = Tuesday, ..., 7 = Sunday
        public bool IsOpen { get; set; } = true;
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int DisplayOrder { get; set; } = 0;

        // Navigation property
        public virtual SystemSetting? Setting { get; set; }
    }
}

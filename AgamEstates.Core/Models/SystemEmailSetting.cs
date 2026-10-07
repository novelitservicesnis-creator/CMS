using System;

namespace AgamEstates.Core.Models
{
    public class SystemEmailSetting
    {
        public int EmailSettingId { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public string ReceiverEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime? LastTestedAt { get; set; }
        public bool? LastTestSucceeded { get; set; }
        public string? LastTestMessage { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
    }
}

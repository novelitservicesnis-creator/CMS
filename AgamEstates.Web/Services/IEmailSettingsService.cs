using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class ResolvedSmtpConfiguration
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string DecryptedPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Agam Estates";
        public string ReceiverEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string Source { get; set; } = "Database";
    }

    public interface IEmailSettingsService
    {
        Task<SystemEmailSettingDto> GetAdminViewSettingsAsync();
        Task<ResolvedSmtpConfiguration> GetResolvedRuntimeConfigAsync();
        Task<string> GetReceiverEmailAsync();
        Task<(bool Success, string Message)> SaveEmailSettingsAsync(UpdateEmailSettingsInputModel input, int? adminUserId);
        Task UpdateTestStatusAsync(bool succeeded, string safeMessage, int? adminUserId);
        string ProtectPassword(string plainPassword);
        string UnprotectPassword(string encryptedPassword);
        string NormalizeAppPassword(string? rawPassword);
        void InvalidateCache();
    }
}

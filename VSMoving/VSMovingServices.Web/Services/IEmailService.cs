using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Services;

public interface IEmailService
{
    Task<SystemEmailSettingsViewModel> GetEmailSettingsViewModelAsync();
    Task<WebsiteSettingsViewModel> GetWebsiteSettingsAsync();
    Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> SaveEmailSettingsAsync(SaveEmailSettingsInputModel input);
    Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> SaveWebsiteSettingsAsync(SaveWebsiteSettingsInputModel input);
    Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> TestConnectionAsync(SaveEmailSettingsInputModel input);
    Task<bool> SendLeadNotificationAsync(Lead lead);
    string BuildLeadEmailHtml(Lead lead);
}

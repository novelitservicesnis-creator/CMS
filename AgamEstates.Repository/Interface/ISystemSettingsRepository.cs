using AgamEstates.Core.Models;
using AgamEstates.Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface ISystemSettingsRepository
    {
        Task<SystemSetting?> GetActiveSettingsAsync();
        Task<SystemSettingDto?> GetSettingsDtoAsync();
        Task<PublicSiteSettingsViewModel> GetPublicSiteSettingsAsync();

        Task<bool> UpdateGeneralSettingsAsync(string companyName, string? logoPath, int? updatedBy);
        Task<bool> UpdateContactInfoAsync(string? addressLine1, string? addressLine2, string? city, string? state, string? postalCode, string? country, string? email, int? updatedBy);
        Task<bool> UpdateSocialLinksAsync(string? facebookUrl, string? instagramUrl, string? youtubeUrl, int? updatedBy);

        Task<List<SystemContactNumberDto>> GetContactNumbersAsync();
        Task<SystemContactNumberDto?> AddContactNumberAsync(string? label, string phoneNumber, bool isPrimary, bool isActive, int sortOrder);
        Task<bool> UpdateContactNumberAsync(int contactNumberId, string? label, string phoneNumber, bool isPrimary, bool isActive, int sortOrder);
        Task<bool> DeleteContactNumberAsync(int contactNumberId);
        Task<bool> SetPrimaryContactNumberAsync(int contactNumberId);
        Task<bool> ToggleContactNumberStatusAsync(int contactNumberId);

        Task<List<SystemBusinessHourDto>> GetBusinessHoursAsync();
        Task<bool> UpdateBusinessHoursAsync(List<SystemBusinessHourDto> hours);

        Task<List<SystemAnnouncementDto>> GetAllAnnouncementsAsync();
        Task<SystemAnnouncementDto?> GetAnnouncementByIdAsync(int announcementId);
        Task<SystemAnnouncementDto?> GetActivePublicAnnouncementAsync();
        Task<int> CreateAnnouncementAsync(string? title, string message, string? linkText, string? linkUrl, DateTime? startDate, DateTime? endDate, bool isActive, int? createdBy);
        Task<bool> UpdateAnnouncementAsync(int announcementId, string? title, string message, string? linkText, string? linkUrl, DateTime? startDate, DateTime? endDate, bool isActive, int? updatedBy);
        Task<bool> ActivateAnnouncementAsync(int announcementId, int? updatedBy);
        Task<bool> DeactivateAnnouncementAsync(int announcementId, int? updatedBy);
        Task<bool> DeleteAnnouncementAsync(int announcementId);

        Task<SystemEmailSetting?> GetActiveEmailSettingEntityAsync();
        Task<SystemEmailSettingDto> GetEmailSettingsDtoAsync();
        Task<bool> SaveEmailSettingsAsync(
            string smtpHost,
            int smtpPort,
            string smtpUsername,
            string? newEncryptedPassword,
            string fromEmail,
            string? fromName,
            string receiverEmail,
            bool enableSsl,
            int? updatedBy);
        Task<bool> UpdateEmailTestStatusAsync(bool succeeded, string? message, int? updatedBy);

        Task EnsureDefaultSettingsSeededAsync();
    }
}

using AgamEstates.Core.Models;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.Service;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class EmailSettingsService : IEmailSettingsService
    {
        public const string DataProtectionPurpose = "AgamEstates.SmtpCredentials.v1";
        private const string CacheKey = "AgamEstates_ActiveEmailSettings";

        private readonly ISystemSettingsRepository _settingsRepository;
        private readonly IDataProtector _protector;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly SmtpSettings _fallbackOptions;
        private readonly ILogger<EmailSettingsService> _logger;

        public EmailSettingsService(
            ISystemSettingsRepository settingsRepository,
            IDataProtectionProvider dataProtectionProvider,
            IMemoryCache cache,
            IConfiguration configuration,
            IOptions<SmtpSettings> fallbackOptions,
            ILogger<EmailSettingsService> logger)
        {
            _settingsRepository = settingsRepository;
            _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
            _cache = cache;
            _configuration = configuration;
            _fallbackOptions = fallbackOptions?.Value ?? new SmtpSettings();
            _logger = logger;
        }

        public async Task<SystemEmailSettingDto> GetAdminViewSettingsAsync()
        {
            return await _settingsRepository.GetEmailSettingsDtoAsync();
        }

        public async Task<ResolvedSmtpConfiguration> GetResolvedRuntimeConfigAsync()
        {
            var dbSetting = await GetCachedActiveDbEntityAsync();

            if (dbSetting != null &&
                dbSetting.IsActive &&
                !string.IsNullOrWhiteSpace(dbSetting.SmtpHost) &&
                dbSetting.SmtpPort > 0 &&
                !string.IsNullOrWhiteSpace(dbSetting.SmtpUsername))
            {
                var decryptedPassword = string.Empty;
                var source = "Database";

                if (!string.IsNullOrWhiteSpace(dbSetting.EncryptedPassword))
                {
                    decryptedPassword = UnprotectPassword(dbSetting.EncryptedPassword);
                }

                // Optional bootstrap fallback for password only if DB password has not been entered yet
                if (string.IsNullOrWhiteSpace(decryptedPassword))
                {
                    var fallbackRawPassword = FirstNonEmpty(_configuration["SmtpSettings:Password"], _fallbackOptions.Password);
                    decryptedPassword = NormalizeAppPassword(fallbackRawPassword);
                    if (!string.IsNullOrWhiteSpace(decryptedPassword))
                    {
                        source = "Database+ConfigurationFallback";
                    }
                }

                var fromEmail = !string.IsNullOrWhiteSpace(dbSetting.FromEmail)
                    ? dbSetting.FromEmail.Trim()
                    : dbSetting.SmtpUsername.Trim();

                var fromName = !string.IsNullOrWhiteSpace(dbSetting.FromName)
                    ? dbSetting.FromName.Trim()
                    : "Agam Estates";

                return new ResolvedSmtpConfiguration
                {
                    SmtpHost = dbSetting.SmtpHost.Trim(),
                    SmtpPort = dbSetting.SmtpPort,
                    SmtpUsername = dbSetting.SmtpUsername.Trim(),
                    DecryptedPassword = decryptedPassword,
                    FromEmail = fromEmail,
                    FromName = fromName,
                    ReceiverEmail = dbSetting.ReceiverEmail?.Trim() ?? string.Empty,
                    EnableSsl = dbSetting.EnableSsl,
                    Source = source
                };
            }

            // Secondary fallback: IConfiguration / User Secrets if DB SMTP configuration does not exist
            var host = FirstNonEmpty(_configuration["SmtpSettings:Host"], _fallbackOptions.Host, "smtp.gmail.com");
            var port = _configuration.GetValue<int?>("SmtpSettings:Port") ?? (_fallbackOptions.Port > 0 ? _fallbackOptions.Port : 587);
            var enableSsl = _configuration.GetValue<bool?>("SmtpSettings:EnableSsl") ?? _fallbackOptions.EnableSsl;
            var username = FirstNonEmpty(_configuration["SmtpSettings:Username"], _fallbackOptions.Username);
            var fallbackFromEmail = FirstNonEmpty(_configuration["SmtpSettings:FromEmail"], _fallbackOptions.FromEmail, username);
            var fallbackFromName = FirstNonEmpty(_configuration["SmtpSettings:FromName"], _fallbackOptions.FromName, "Agam Estates");
            var fallbackPassword = NormalizeAppPassword(FirstNonEmpty(_configuration["SmtpSettings:Password"], _fallbackOptions.Password));

            return new ResolvedSmtpConfiguration
            {
                SmtpHost = host,
                SmtpPort = port,
                SmtpUsername = username,
                DecryptedPassword = fallbackPassword,
                FromEmail = fallbackFromEmail,
                FromName = fallbackFromName,
                ReceiverEmail = string.Empty,
                EnableSsl = enableSsl,
                Source = "ConfigurationFallback"
            };
        }

        public async Task<string> GetReceiverEmailAsync()
        {
            var resolved = await GetResolvedRuntimeConfigAsync();
            if (!string.IsNullOrWhiteSpace(resolved.ReceiverEmail) && IsValidEmail(resolved.ReceiverEmail))
            {
                return resolved.ReceiverEmail.Trim();
            }

            // Secondary fallback: general SystemSettings.Email from DB
            var publicSettings = await _settingsRepository.GetPublicSiteSettingsAsync();
            if (!string.IsNullOrWhiteSpace(publicSettings?.Email) && IsValidEmail(publicSettings.Email))
            {
                return publicSettings.Email.Trim();
            }

            return string.Empty;
        }

        public async Task<(bool Success, string Message)> SaveEmailSettingsAsync(UpdateEmailSettingsInputModel input, int? adminUserId)
        {
            if (input == null)
            {
                return (false, "Invalid email settings payload.");
            }

            var host = input.SmtpHost?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(host) || host.Length > 200)
            {
                return (false, "SMTP Host is required and cannot exceed 200 characters.");
            }

            if (input.SmtpPort < 1 || input.SmtpPort > 65535)
            {
                return (false, "SMTP Port must be between 1 and 65535.");
            }

            var username = input.SmtpUsername?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || !IsValidEmail(username))
            {
                return (false, "Please provide a valid Sender Email / SMTP Username.");
            }

            var fromEmail = !string.IsNullOrWhiteSpace(input.FromEmail)
                ? input.FromEmail.Trim()
                : username;

            if (!IsValidEmail(fromEmail))
            {
                return (false, "Please provide a valid From Email address.");
            }

            var fromName = !string.IsNullOrWhiteSpace(input.FromName)
                ? input.FromName.Trim()
                : "Agam Estates";

            if (fromName.Length > 150)
            {
                return (false, "From Name cannot exceed 150 characters.");
            }

            var receiverEmail = input.ReceiverEmail?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(receiverEmail) || !IsValidEmail(receiverEmail))
            {
                return (false, "Please provide a valid Receiver Email address.");
            }

            var existingEntity = await _settingsRepository.GetActiveEmailSettingEntityAsync();
            bool hasExistingPassword = !string.IsNullOrWhiteSpace(existingEntity?.EncryptedPassword);

            var normalizedNewPassword = NormalizeAppPassword(input.SmtpPassword);
            string? encryptedPasswordToSave = null;

            if (!string.IsNullOrWhiteSpace(normalizedNewPassword))
            {
                encryptedPasswordToSave = ProtectPassword(normalizedNewPassword);
            }
            else if (!hasExistingPassword)
            {
                return (false, "SMTP App Password is required for initial setup.");
            }

            var saved = await _settingsRepository.SaveEmailSettingsAsync(
                smtpHost: host,
                smtpPort: input.SmtpPort,
                smtpUsername: username,
                newEncryptedPassword: encryptedPasswordToSave,
                fromEmail: fromEmail,
                fromName: fromName,
                receiverEmail: receiverEmail,
                enableSsl: input.EnableSsl,
                updatedBy: adminUserId);

            InvalidateCache();

            return saved
                ? (true, "Email notification settings saved successfully.")
                : (false, "Unable to save email notification settings.");
        }

        public async Task UpdateTestStatusAsync(bool succeeded, string safeMessage, int? adminUserId)
        {
            await _settingsRepository.UpdateEmailTestStatusAsync(succeeded, safeMessage, adminUserId);
            InvalidateCache();
        }

        public string ProtectPassword(string plainPassword)
        {
            var normalized = NormalizeAppPassword(plainPassword);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            return _protector.Protect(normalized);
        }

        public string UnprotectPassword(string encryptedPassword)
        {
            if (string.IsNullOrWhiteSpace(encryptedPassword))
            {
                return string.Empty;
            }

            try
            {
                return _protector.Unprotect(encryptedPassword);
            }
            catch (CryptographicException)
            {
                // Fallback support if encrypted using EncryptionService
                try
                {
                    return EncryptionService.Decrypt(encryptedPassword, DataProtectionPurpose);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to decrypt stored SMTP password. Please re-enter and save the SMTP App Password in System Settings.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while unprotecting stored SMTP credential.");
                return string.Empty;
            }
        }

        public string NormalizeAppPassword(string? rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                return string.Empty;
            }

            return Regex.Replace(rawPassword, @"\s+", string.Empty);
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }

        private async Task<SystemEmailSetting?> GetCachedActiveDbEntityAsync()
        {
            if (_cache.TryGetValue(CacheKey, out SystemEmailSetting? cached) && cached != null)
            {
                return cached;
            }

            var entity = await _settingsRepository.GetActiveEmailSettingEntityAsync();
            if (entity != null)
            {
                var copy = new SystemEmailSetting
                {
                    EmailSettingId = entity.EmailSettingId,
                    SmtpHost = entity.SmtpHost,
                    SmtpPort = entity.SmtpPort,
                    SmtpUsername = entity.SmtpUsername,
                    EncryptedPassword = entity.EncryptedPassword,
                    FromEmail = entity.FromEmail,
                    FromName = entity.FromName,
                    ReceiverEmail = entity.ReceiverEmail,
                    EnableSsl = entity.EnableSsl,
                    IsActive = entity.IsActive,
                    LastTestedAt = entity.LastTestedAt,
                    LastTestSucceeded = entity.LastTestSucceeded,
                    LastTestMessage = entity.LastTestMessage,
                    UpdatedAt = entity.UpdatedAt,
                    UpdatedBy = entity.UpdatedBy
                };

                _cache.Set(CacheKey, copy, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });

                return copy;
            }

            return null;
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > 200)
            {
                return false;
            }

            var trimmed = email.Trim();
            if (trimmed.IndexOfAny(new[] { '\r', '\n', '\0', ' ' }) >= 0)
            {
                return false;
            }

            var atIndex = trimmed.IndexOf('@');
            var lastDotIndex = trimmed.LastIndexOf('.');
            if (atIndex <= 0 || lastDotIndex <= atIndex + 1 || lastDotIndex >= trimmed.Length - 2)
            {
                return false;
            }

            return MailAddress.TryCreate(trimmed, out var parsed) &&
                   string.Equals(parsed.Address, trimmed, StringComparison.OrdinalIgnoreCase);
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var v in values)
            {
                if (!string.IsNullOrWhiteSpace(v))
                {
                    return v.Trim();
                }
            }
            return string.Empty;
        }
    }
}

using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Services;

public class EmailService : IEmailService
{
    private const string ProtectorPurpose = "VSMovingServices.SmtpCredentials.v1";
    private readonly VSMovingDbContext _db;
    private readonly IDataProtector _protector;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        VSMovingDbContext db,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<EmailService> logger)
    {
        _db = db;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _logger = logger;
    }

    private async Task<SystemEmailSetting> GetOrCreateEntityAsync()
    {
        var setting = await _db.SystemEmailSettings.OrderBy(s => s.Id).FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new SystemEmailSetting
            {
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                SenderEmail = "vsshippingservicespvt@gmail.com",
                FromName = "VS Moving Services",
                FromEmail = "vsshippingservicespvt@gmail.com",
                ReceiverEmail = "vsshippingservicespvt@gmail.com",
                EnableSsl = true,
                ConfigurationChanged = false,
                FacebookUrl = null,
                InstagramUrl = null,
                WhatsAppNumber = "15149999790",
                WebsiteEmail = "vsshippingservicespvt@gmail.com",
                PrimaryPhone = "514 999 9790",
                SecondaryPhone = "438 990 8055",
                BusinessAddress = "Halifax, Nova Scotia",
                UpdatedAt = DateTime.Now
            };
            _db.SystemEmailSettings.Add(setting);
            await _db.SaveChangesAsync();
        }
        return setting;
    }

    public async Task<SystemEmailSettingsViewModel> GetEmailSettingsViewModelAsync()
    {
        var entity = await GetOrCreateEntityAsync();
        return MapToViewModel(entity);
    }

    public async Task<WebsiteSettingsViewModel> GetWebsiteSettingsAsync()
    {
        var entity = await GetOrCreateEntityAsync();
        return new WebsiteSettingsViewModel
        {
            FacebookUrl = NullIfWhiteSpace(entity.FacebookUrl),
            InstagramUrl = NullIfWhiteSpace(entity.InstagramUrl),
            WhatsAppNumber = NullIfWhiteSpace(entity.WhatsAppNumber),
            WebsiteEmail = NullIfWhiteSpace(entity.WebsiteEmail),
            PrimaryPhone = NullIfWhiteSpace(entity.PrimaryPhone),
            SecondaryPhone = NullIfWhiteSpace(entity.SecondaryPhone),
            BusinessAddress = NullIfWhiteSpace(entity.BusinessAddress)
        };
    }

    private static SystemEmailSettingsViewModel MapToViewModel(SystemEmailSetting entity)
    {
        string status;
        if (entity.ConfigurationChanged)
        {
            status = "Configuration Changed";
        }
        else if (entity.LastTestSucceeded == true)
        {
            status = "Valid";
        }
        else if (entity.LastTestSucceeded == false)
        {
            status = "Invalid";
        }
        else
        {
            status = "Not Tested";
        }

        return new SystemEmailSettingsViewModel
        {
            SmtpHost = entity.SmtpHost,
            SmtpPort = entity.SmtpPort,
            SenderEmail = entity.SenderEmail,
            FromName = entity.FromName,
            FromEmail = entity.FromEmail,
            ReceiverEmail = entity.ReceiverEmail,
            EnableSsl = entity.EnableSsl,
            HasStoredPassword = !string.IsNullOrWhiteSpace(entity.EncryptedSmtpPassword),
            ConnectionStatus = status,
            LastTestedAtFormatted = entity.LastTestedAt?.ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture),
            LastTestSucceeded = entity.LastTestSucceeded,
            LastTestMessage = entity.LastTestMessage,
            FacebookUrl = entity.FacebookUrl,
            InstagramUrl = entity.InstagramUrl,
            WhatsAppNumber = entity.WhatsAppNumber,
            WebsiteEmail = entity.WebsiteEmail,
            PrimaryPhone = entity.PrimaryPhone,
            SecondaryPhone = entity.SecondaryPhone,
            BusinessAddress = entity.BusinessAddress
        };
    }

    public async Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> SaveWebsiteSettingsAsync(SaveWebsiteSettingsInputModel input)
    {
        var entity = await GetOrCreateEntityAsync();

        var facebookUrl = NullIfWhiteSpace(input.FacebookUrl);
        var instagramUrl = NullIfWhiteSpace(input.InstagramUrl);
        var whatsAppNumber = NullIfWhiteSpace(input.WhatsAppNumber);
        var websiteEmail = NullIfWhiteSpace(input.WebsiteEmail);
        var primaryPhone = NullIfWhiteSpace(input.PrimaryPhone);
        var secondaryPhone = NullIfWhiteSpace(input.SecondaryPhone);
        var businessAddress = NullIfWhiteSpace(input.BusinessAddress);

        if (facebookUrl != null)
        {
            if (facebookUrl.Length > 300 || !IsValidHttpUrl(facebookUrl))
                return (false, "Please enter a valid Facebook URL starting with http:// or https://.", MapToViewModel(entity));
        }

        if (instagramUrl != null)
        {
            if (instagramUrl.Length > 300 || !IsValidHttpUrl(instagramUrl))
                return (false, "Please enter a valid Instagram URL starting with http:// or https://.", MapToViewModel(entity));
        }

        if (websiteEmail != null)
        {
            if (websiteEmail.Length > 150 || !IsValidEmail(websiteEmail))
                return (false, "Please enter a valid Website Email address.", MapToViewModel(entity));
        }

        if (primaryPhone != null)
        {
            if (primaryPhone.Length > 40 || !IsValidPhone(primaryPhone))
                return (false, "Please enter a valid Primary Phone number (at least 7 digits).", MapToViewModel(entity));
        }

        if (secondaryPhone != null)
        {
            if (secondaryPhone.Length > 40 || !IsValidPhone(secondaryPhone))
                return (false, "Please enter a valid Secondary Phone number (at least 7 digits).", MapToViewModel(entity));
        }

        if (whatsAppNumber != null)
        {
            if (whatsAppNumber.Length > 40 || !IsValidPhone(whatsAppNumber))
                return (false, "Please enter a valid WhatsApp Number (at least 7 digits).", MapToViewModel(entity));
        }

        if (businessAddress != null && businessAddress.Length > 250)
        {
            return (false, "Business Address cannot exceed 250 characters.", MapToViewModel(entity));
        }

        entity.FacebookUrl = facebookUrl;
        entity.InstagramUrl = instagramUrl;
        entity.WhatsAppNumber = whatsAppNumber;
        entity.WebsiteEmail = websiteEmail;
        entity.PrimaryPhone = primaryPhone;
        entity.SecondaryPhone = secondaryPhone;
        entity.BusinessAddress = businessAddress;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return (true, "Website & social settings saved successfully.", MapToViewModel(entity));
    }

    public async Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> SaveEmailSettingsAsync(SaveEmailSettingsInputModel input)
    {
        var entity = await GetOrCreateEntityAsync();

        var host = input.SmtpHost?.Trim() ?? string.Empty;
        var senderEmail = input.SenderEmail?.Trim() ?? string.Empty;
        var receiverEmail = input.ReceiverEmail?.Trim() ?? string.Empty;
        var fromName = string.IsNullOrWhiteSpace(input.FromName) ? "VS Moving Services" : input.FromName.Trim();
        var fromEmail = string.IsNullOrWhiteSpace(input.FromEmail) ? senderEmail : input.FromEmail.Trim();

        if (string.IsNullOrWhiteSpace(host))
            return (false, "SMTP Host is required.", MapToViewModel(entity));

        if (input.SmtpPort < 1 || input.SmtpPort > 65535)
            return (false, "SMTP Port must be between 1 and 65535.", MapToViewModel(entity));

        if (!IsValidEmail(senderEmail))
            return (false, "Please enter a valid Sender Email address.", MapToViewModel(entity));

        if (!IsValidEmail(receiverEmail))
            return (false, "Please enter a valid Receiver Email address.", MapToViewModel(entity));

        if (!string.IsNullOrWhiteSpace(fromEmail) && !IsValidEmail(fromEmail))
            return (false, "Please enter a valid From Email address.", MapToViewModel(entity));

        bool hasExistingPassword = !string.IsNullOrWhiteSpace(entity.EncryptedSmtpPassword);
        bool hasNewPassword = !string.IsNullOrWhiteSpace(input.SmtpPassword);

        if (!hasExistingPassword && !hasNewPassword)
        {
            return (false, "SMTP App Password is required for initial configuration.", MapToViewModel(entity));
        }

        // Check if any connection-critical setting changed after a successful test
        bool connectionCriticalChanged =
            !string.Equals(entity.SmtpHost, host, StringComparison.OrdinalIgnoreCase) ||
            entity.SmtpPort != input.SmtpPort ||
            !string.Equals(entity.SenderEmail, senderEmail, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(entity.FromEmail ?? entity.SenderEmail, fromEmail, StringComparison.OrdinalIgnoreCase) ||
            entity.EnableSsl != input.EnableSsl ||
            hasNewPassword;

        entity.SmtpHost = host;
        entity.SmtpPort = input.SmtpPort;
        entity.SenderEmail = senderEmail;
        entity.FromName = fromName;
        entity.FromEmail = fromEmail;
        entity.ReceiverEmail = receiverEmail;
        entity.EnableSsl = input.EnableSsl;

        // Save Existing Password Rule: only overwrite if a new non-whitespace password was provided
        if (hasNewPassword)
        {
            var normalizedPwd = NormalizeAppPassword(input.SmtpPassword!);
            entity.EncryptedSmtpPassword = _protector.Protect(normalizedPwd);
        }

        if (connectionCriticalChanged && entity.LastTestSucceeded == true)
        {
            entity.ConfigurationChanged = true;
            entity.LastTestMessage = "Configuration was modified since the last successful connection test.";
        }

        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return (true, "Email notification settings saved securely.", MapToViewModel(entity));
    }

    public async Task<(bool Success, string Message, SystemEmailSettingsViewModel ViewModel)> TestConnectionAsync(SaveEmailSettingsInputModel input)
    {
        var entity = await GetOrCreateEntityAsync();

        var host = !string.IsNullOrWhiteSpace(input.SmtpHost) ? input.SmtpHost.Trim() : entity.SmtpHost;
        var port = input.SmtpPort > 0 ? input.SmtpPort : entity.SmtpPort;
        var senderEmail = !string.IsNullOrWhiteSpace(input.SenderEmail) ? input.SenderEmail.Trim() : entity.SenderEmail;
        var receiverEmail = !string.IsNullOrWhiteSpace(input.ReceiverEmail) ? input.ReceiverEmail.Trim() : entity.ReceiverEmail;
        var fromName = !string.IsNullOrWhiteSpace(input.FromName) ? input.FromName.Trim() : (entity.FromName ?? "VS Moving Services");
        var fromEmail = !string.IsNullOrWhiteSpace(input.FromEmail) ? input.FromEmail.Trim() : senderEmail;
        var enableSsl = input.EnableSsl;

        if (string.IsNullOrWhiteSpace(host) || port < 1 || port > 65535)
            return (false, "Valid SMTP Host and Port are required to test the connection.", MapToViewModel(entity));

        if (!IsValidEmail(senderEmail) || !IsValidEmail(receiverEmail))
            return (false, "Valid Sender Email and Receiver Email are required to test the connection.", MapToViewModel(entity));

        string? plainPassword = null;
        if (!string.IsNullOrWhiteSpace(input.SmtpPassword))
        {
            plainPassword = NormalizeAppPassword(input.SmtpPassword);
        }
        else if (!string.IsNullOrWhiteSpace(entity.EncryptedSmtpPassword))
        {
            plainPassword = UnprotectSafe(entity.EncryptedSmtpPassword);
        }

        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            entity.LastTestedAt = DateTime.Now;
            entity.LastTestSucceeded = false;
            entity.ConfigurationChanged = false;
            entity.LastTestMessage = "SMTP App Password is not configured.";
            await _db.SaveChangesAsync();
            return (false, "SMTP App Password is required to test the connection.", MapToViewModel(entity));
        }

        // Persist the tested configuration if new password or fields were supplied
        entity.SmtpHost = host;
        entity.SmtpPort = port;
        entity.SenderEmail = senderEmail;
        entity.FromName = fromName;
        entity.FromEmail = fromEmail;
        entity.ReceiverEmail = receiverEmail;
        entity.EnableSsl = enableSsl;
        if (!string.IsNullOrWhiteSpace(input.SmtpPassword))
        {
            entity.EncryptedSmtpPassword = _protector.Protect(plainPassword);
        }

        try
        {
            var testHtml = @"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /></head>
<body style=""margin:0;padding:24px;background-color:#f4f5f7;font-family:'Inter',Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:560px;margin:0 auto;background:#ffffff;border:1px solid #e7e8eb;border-radius:12px;overflow:hidden;"">
    <tr>
      <td style=""background:#131316;padding:22px 28px;border-bottom:4px solid #e01e26;"">
        <div style=""font-size:20px;font-weight:800;letter-spacing:1px;color:#ffffff;"">VS MOVING <span style=""color:#e01e26;"">SERVICES</span></div>
        <div style=""font-size:11px;letter-spacing:2px;color:#9ca3af;margin-top:4px;text-transform:uppercase;"">SMTP Connection Test</div>
      </td>
    </tr>
    <tr>
      <td style=""padding:28px;color:#131316;font-size:14px;line-height:1.6;"">
        <p style=""margin:0 0 10px 0;font-weight:700;font-size:16px;"">SMTP Configuration Verified</p>
        <p style=""margin:0;color:#5b6270;"">Your VS Moving Services email notification settings are working properly. Website enquiry alerts will be delivered to this inbox.</p>
      </td>
    </tr>
  </table>
</body>
</html>";

            await SendRawSmtpAsync(
                host,
                port,
                enableSsl,
                senderEmail,
                plainPassword,
                fromEmail,
                fromName,
                receiverEmail,
                "VS Moving Services — SMTP Connection Test",
                testHtml,
                replyToEmail: null,
                replyToName: null);

            entity.LastTestedAt = DateTime.Now;
            entity.LastTestSucceeded = true;
            entity.ConfigurationChanged = false;
            entity.LastTestMessage = $"Connection verified and test email sent to {receiverEmail}.";
            entity.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return (true, $"SMTP connection verified. A test email was sent to {receiverEmail}.", MapToViewModel(entity));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP test connection failed for host {Host}:{Port} and user {SenderEmail}.", host, port, senderEmail);

            entity.LastTestedAt = DateTime.Now;
            entity.LastTestSucceeded = false;
            entity.ConfigurationChanged = false;
            entity.LastTestMessage = "Unable to authenticate or connect to the SMTP server. Please verify your host, port, SSL setting, and App Password.";
            entity.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return (false, entity.LastTestMessage, MapToViewModel(entity));
        }
    }

    public async Task<bool> SendLeadNotificationAsync(Lead lead)
    {
        try
        {
            var entity = await GetOrCreateEntityAsync();
            if (string.IsNullOrWhiteSpace(entity.EncryptedSmtpPassword) ||
                string.IsNullOrWhiteSpace(entity.SmtpHost) ||
                string.IsNullOrWhiteSpace(entity.SenderEmail) ||
                string.IsNullOrWhiteSpace(entity.ReceiverEmail))
            {
                _logger.LogInformation("Lead #{LeadId} saved. Skipping email notification because SMTP credentials are not yet configured.", lead.LeadId);
                return false;
            }

            var plainPassword = UnprotectSafe(entity.EncryptedSmtpPassword);
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                _logger.LogWarning("Lead #{LeadId} saved, but stored SMTP password could not be unprotected.", lead.LeadId);
                return false;
            }

            var subject = $"New Website Enquiry — {lead.FullName}";
            var htmlBody = BuildLeadEmailHtml(lead);
            var fromEmail = !string.IsNullOrWhiteSpace(entity.FromEmail) ? entity.FromEmail : entity.SenderEmail;
            var fromName = !string.IsNullOrWhiteSpace(entity.FromName) ? entity.FromName : "VS Moving Services";

            await SendRawSmtpAsync(
                entity.SmtpHost,
                entity.SmtpPort,
                entity.EnableSsl,
                entity.SenderEmail,
                plainPassword,
                fromEmail,
                fromName,
                entity.ReceiverEmail,
                subject,
                htmlBody,
                lead.Email,
                lead.FullName);

            _logger.LogInformation("Lead notification email sent successfully for Lead #{LeadId}.", lead.LeadId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send lead notification email for Lead #{LeadId}. Lead remains saved in database.", lead.LeadId);
            return false;
        }
    }

    public string BuildLeadEmailHtml(Lead lead)
    {
        var hasPhone = !string.IsNullOrWhiteSpace(lead.PhoneNumber);
        var hasEmail = !string.IsNullOrWhiteSpace(lead.Email);
        var hasMessage = !string.IsNullOrWhiteSpace(lead.Message);

        var safeName = WebUtility.HtmlEncode(lead.FullName);
        var safePhone = hasPhone ? WebUtility.HtmlEncode(lead.PhoneNumber!.Trim()) : string.Empty;
        var safeEmail = hasEmail ? WebUtility.HtmlEncode(lead.Email!.Trim()) : string.Empty;
        var safeMessage = hasMessage ? WebUtility.HtmlEncode(lead.Message!.Trim()).Replace("\n", "<br />") : string.Empty;
        var safeSource = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(lead.Source) ? "VS Moving Services Website" : lead.Source);
        var submittedDate = lead.CreatedAt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var submittedTime = lead.CreatedAt.ToString("hh:mm tt", CultureInfo.InvariantCulture);

        var sb = new StringBuilder();
        sb.Append(@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /></head>
<body style=""margin:0;padding:24px;background-color:#f4f5f7;font-family:'Inter',Arial,sans-serif;color:#131316;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e7e8eb;border-radius:14px;overflow:hidden;"">
    <tr>
      <td style=""background:#131316;padding:24px 30px;border-bottom:4px solid #e01e26;"">
        <div style=""font-size:20px;font-weight:800;letter-spacing:1px;color:#ffffff;"">VS MOVING <span style=""color:#e01e26;"">SERVICES</span></div>
        <div style=""font-size:12px;font-weight:700;letter-spacing:2px;color:#e01e26;margin-top:6px;text-transform:uppercase;"">NEW LEAD RECEIVED</div>
      </td>
    </tr>
    <tr>
      <td style=""padding:30px;"">
        <p style=""margin:0 0 10px 0;font-size:15px;font-weight:600;color:#131316;"">Hello Team,</p>
        <p style=""margin:0 0 24px 0;font-size:14px;color:#5b6270;line-height:1.6;"">A new customer has submitted an enquiry through the VS Moving Services website.</p>

        <div style=""font-size:11px;font-weight:700;letter-spacing:1.5px;color:#e01e26;text-transform:uppercase;margin-bottom:10px;"">Customer Details</div>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f5f7;border:1px solid #e7e8eb;border-radius:10px;padding:16px;margin-bottom:22px;"">
          <tr>
            <td style=""padding:6px 0;"">
              <div style=""font-size:11px;color:#5b6270;text-transform:uppercase;letter-spacing:0.8px;"">Full Name</div>
              <div style=""font-size:15px;font-weight:700;color:#131316;margin-top:2px;"">").Append(safeName).Append(@"</div>
            </td>
          </tr>");

        if (hasPhone)
        {
            sb.Append(@"
          <tr>
            <td style=""padding:8px 0 6px 0;border-top:1px solid #e7e8eb;"">
              <div style=""font-size:11px;color:#5b6270;text-transform:uppercase;letter-spacing:0.8px;"">Phone Number</div>
              <div style=""font-size:14px;font-weight:600;color:#131316;margin-top:2px;"">").Append(safePhone).Append(@"</div>
            </td>
          </tr>");
        }

        if (hasEmail)
        {
            sb.Append(@"
          <tr>
            <td style=""padding:8px 0 6px 0;border-top:1px solid #e7e8eb;"">
              <div style=""font-size:11px;color:#5b6270;text-transform:uppercase;letter-spacing:0.8px;"">Email Address</div>
              <div style=""font-size:14px;font-weight:600;color:#131316;margin-top:2px;"">").Append(safeEmail).Append(@"</div>
            </td>
          </tr>");
        }

        sb.Append(@"
        </table>");

        if (hasMessage)
        {
            sb.Append(@"
        <div style=""font-size:11px;font-weight:700;letter-spacing:1.5px;color:#e01e26;text-transform:uppercase;margin-bottom:10px;"">Customer Message</div>
        <div style=""background:#f4f5f7;border-left:4px solid #e01e26;border-radius:8px;padding:16px;font-size:14px;color:#131316;line-height:1.6;margin-bottom:22px;"">")
              .Append(safeMessage)
              .Append(@"</div>");
        }

        sb.Append(@"
        <div style=""font-size:11px;font-weight:700;letter-spacing:1.5px;color:#e01e26;text-transform:uppercase;margin-bottom:10px;"">Enquiry Information</div>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e7e8eb;border-radius:10px;padding:16px;margin-bottom:24px;"">
          <tr>
            <td style=""padding:4px 0;font-size:13px;color:#5b6270;"">Source</td>
            <td style=""padding:4px 0;font-size:13px;font-weight:600;color:#131316;text-align:right;"">").Append(safeSource).Append(@"</td>
          </tr>
          <tr>
            <td style=""padding:4px 0;font-size:13px;color:#5b6270;"">Submitted On</td>
            <td style=""padding:4px 0;font-size:13px;font-weight:600;color:#131316;text-align:right;"">").Append(submittedDate).Append(@"</td>
          </tr>
          <tr>
            <td style=""padding:4px 0;font-size:13px;color:#5b6270;"">Submitted At</td>
            <td style=""padding:4px 0;font-size:13px;font-weight:600;color:#131316;text-align:right;"">").Append(submittedTime).Append(@"</td>
          </tr>
          <tr>
            <td style=""padding:4px 0;font-size:13px;color:#5b6270;"">Reference</td>
            <td style=""padding:4px 0;font-size:13px;font-weight:700;color:#e01e26;text-align:right;"">#").Append(lead.LeadId).Append(@"</td>
          </tr>
        </table>");

        if (hasPhone || hasEmail)
        {
            sb.Append(@"<div style=""margin-top:18px;"">");
            if (hasPhone)
            {
                var telHref = new string(lead.PhoneNumber!.Where(c => char.IsDigit(c) || c == '+').ToArray());
                sb.Append(@"<a href=""tel:").Append(telHref).Append(@""" style=""display:inline-block;background:#e01e26;color:#ffffff;text-decoration:none;font-size:13px;font-weight:700;padding:12px 20px;border-radius:8px;margin-right:10px;margin-bottom:8px;"">CALL CUSTOMER &middot; ").Append(safePhone).Append(@"</a>");
            }
            if (hasEmail)
            {
                sb.Append(@"<a href=""mailto:").Append(safeEmail).Append(@""" style=""display:inline-block;background:#131316;color:#ffffff;text-decoration:none;font-size:13px;font-weight:700;padding:12px 20px;border-radius:8px;margin-bottom:8px;"">REPLY BY EMAIL &middot; ").Append(safeEmail).Append(@"</a>");
            }
            sb.Append(@"</div>");
        }

        sb.Append(@"
      </td>
    </tr>
  </table>
</body>
</html>");

        return sb.ToString();
    }

    private static async Task SendRawSmtpAsync(
        string host,
        int port,
        bool enableSsl,
        string username,
        string password,
        string fromEmail,
        string fromName,
        string toEmail,
        string subject,
        string htmlBody,
        string? replyToEmail,
        string? replyToName)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName, Encoding.UTF8),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = htmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        if (!string.IsNullOrWhiteSpace(replyToEmail) && MailAddress.TryCreate(replyToEmail.Trim(), out var replyAddr))
        {
            message.ReplyToList.Add(!string.IsNullOrWhiteSpace(replyToName)
                ? new MailAddress(replyAddr.Address, replyToName.Trim(), Encoding.UTF8)
                : replyAddr);
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };

        await client.SendMailAsync(message);
    }

    private string? UnprotectSafe(string encryptedValue)
    {
        try
        {
            return _protector.Unprotect(encryptedValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to unprotect stored SMTP password.");
            return null;
        }
    }

    private static string NormalizeAppPassword(string raw)
    {
        return raw.Trim().Replace(" ", string.Empty);
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return MailAddress.TryCreate(email.Trim(), out _);
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               !string.IsNullOrWhiteSpace(uri.Host);
    }

    private static bool IsValidPhone(string phone)
    {
        var digitCount = phone.Count(char.IsDigit);
        if (digitCount < 7 || digitCount > 20)
            return false;

        return phone.All(c => char.IsDigit(c) || c == ' ' || c == '+' || c == '-' || c == '(' || c == ')' || c == '.');
    }
}

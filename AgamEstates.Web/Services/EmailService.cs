using AgamEstates.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IEmailSettingsService emailSettingsService,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _emailSettingsService = emailSettingsService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody,
            string? replyToEmail = null,
            string? replyToName = null)
        {
            var resolved = await _emailSettingsService.GetResolvedRuntimeConfigAsync();
            await SendWithConfigurationAsync(
                resolved,
                recipientEmail,
                subject,
                htmlBody,
                replyToEmail,
                replyToName);
        }

        public async Task SendTestConnectionEmailAsync(ResolvedSmtpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            const string testSubject = "Agam Estates — Email Configuration Test";
            var testHtmlBody = @"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /></head>
<body style=""margin:0;padding:24px;background-color:#F8F6F0;font-family:'Segoe UI',Arial,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;margin:0 auto;background-color:#FFFFFF;border:1px solid #E4DEC9;border-radius:4px;overflow:hidden;"">
        <tr>
            <td style=""background-color:#041A33;padding:24px 28px;border-bottom:3px solid #DDAE45;"">
                <div style=""font-family:Georgia,'Times New Roman',serif;font-size:20px;font-weight:700;letter-spacing:2px;color:#FFFFFF;text-transform:uppercase;"">
                    AGAM <span style=""color:#DDAE45;"">ESTATES</span>
                </div>
                <div style=""font-size:11px;letter-spacing:1.2px;color:#F5D98A;text-transform:uppercase;margin-top:4px;"">
                    System Settings Verification
                </div>
            </td>
        </tr>
        <tr>
            <td style=""padding:28px;color:#041A33;font-size:15px;line-height:1.6;"">
                <p style=""margin:0 0 12px 0;font-weight:600;color:#062545;"">Agam Estates email notification configuration is working correctly.</p>
                <p style=""margin:0;font-size:13.5px;color:#6A7B95;"">Website enquiry notifications will now be delivered using this active SMTP configuration.</p>
            </td>
        </tr>
    </table>
</body>
</html>";

            await SendWithConfigurationAsync(
                config,
                config.ReceiverEmail,
                testSubject,
                testHtmlBody);
        }

        private async Task SendWithConfigurationAsync(
            ResolvedSmtpConfiguration config,
            string recipientEmail,
            string subject,
            string htmlBody,
            string? replyToEmail = null,
            string? replyToName = null)
        {
            var sanitizedRecipient = SanitizeHeaderValue(recipientEmail);
            if (string.IsNullOrWhiteSpace(sanitizedRecipient) || !MailAddress.TryCreate(sanitizedRecipient, out var toAddress))
            {
                throw new ArgumentException("A valid recipient email address is required.", nameof(recipientEmail));
            }

            var host = SanitizeHeaderValue(config.SmtpHost);
            var port = config.SmtpPort;
            var enableSsl = config.EnableSsl;
            var username = SanitizeHeaderValue(config.SmtpUsername);
            var fromEmail = SanitizeHeaderValue(!string.IsNullOrWhiteSpace(config.FromEmail) ? config.FromEmail : username);
            var fromName = SanitizeHeaderValue(!string.IsNullOrWhiteSpace(config.FromName) ? config.FromName : "Agam Estates");

            if (string.IsNullOrWhiteSpace(host) || port < 1 || port > 65535)
            {
                throw new InvalidOperationException("SMTP Host and Port are not configured properly.");
            }

            if (string.IsNullOrWhiteSpace(username) || !MailAddress.TryCreate(fromEmail, out _))
            {
                throw new InvalidOperationException("SMTP Sender Email / Username is not configured properly.");
            }

            var smtpPassword = NormalizeAppPassword(config.DecryptedPassword);
            if (string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new InvalidOperationException(
                    "SMTP App Password is not configured. Please configure it in Admin -> System Settings -> Email Notifications.");
            }

            var safeSubject = SanitizeHeaderValue(subject);

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName, Encoding.UTF8),
                Subject = safeSubject,
                SubjectEncoding = Encoding.UTF8,
                Body = htmlBody ?? string.Empty,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toAddress);

            // Configure Reply-To to the customer's submitted email without spoofing From
            if (!string.IsNullOrWhiteSpace(replyToEmail))
            {
                var safeReplyToEmail = SanitizeHeaderValue(replyToEmail);
                var safeReplyToName = SanitizeHeaderValue(replyToName);

                if (MailAddress.TryCreate(safeReplyToEmail, out _))
                {
                    var replyToAddress = !string.IsNullOrWhiteSpace(safeReplyToName)
                        ? new MailAddress(safeReplyToEmail, safeReplyToName, Encoding.UTF8)
                        : new MailAddress(safeReplyToEmail);
                    mailMessage.ReplyToList.Add(replyToAddress);
                }
            }

            var pickupDir = _configuration["SmtpSettings:PickupDirectoryLocation"];
            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, smtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000
            };

            if (!string.IsNullOrWhiteSpace(pickupDir))
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtpClient.PickupDirectoryLocation = pickupDir;
                smtpClient.EnableSsl = false;
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

        public string BuildEnquirySubject(string? fullName)
        {
            var cleanName = SanitizeSingleLineValue(fullName);
            return !string.IsNullOrWhiteSpace(cleanName)
                ? $"New Website Enquiry — {cleanName}"
                : "New Website Enquiry";
        }

        public string BuildEnquiryNotificationHtml(EnquiryEmailNotificationModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 1. Sanitize & HTML-encode dynamic customer fields (no placeholder fallbacks)
            var cleanFullName = SanitizeSingleLineValue(model.FullName);
            bool hasFullName = !string.IsNullOrWhiteSpace(cleanFullName);
            var encodedFullName = hasFullName ? WebUtility.HtmlEncode(cleanFullName) : string.Empty;
            var encodedDocumentTitle = hasFullName
                ? $"New Website Enquiry — {encodedFullName}"
                : "New Website Enquiry";

            var cleanPhone = SanitizeSingleLineValue(model.PhoneNumber);
            var dialablePhone = Regex.Replace(cleanPhone, @"[^\d+]", string.Empty);
            if (string.IsNullOrWhiteSpace(dialablePhone) && !string.IsNullOrWhiteSpace(cleanPhone))
            {
                dialablePhone = Regex.Replace(cleanPhone, @"\s+", string.Empty);
            }
            bool hasPhone = !string.IsNullOrWhiteSpace(cleanPhone) && !string.IsNullOrWhiteSpace(dialablePhone);
            var encodedPhone = hasPhone ? WebUtility.HtmlEncode(cleanPhone) : string.Empty;
            var encodedTelHref = hasPhone ? WebUtility.HtmlEncode(dialablePhone) : string.Empty;

            var cleanEmail = SanitizeHeaderValue(model.Email);
            bool hasEmail = !string.IsNullOrWhiteSpace(cleanEmail);
            var encodedEmail = hasEmail ? WebUtility.HtmlEncode(cleanEmail) : string.Empty;
            var encodedMailtoHref = hasEmail ? WebUtility.HtmlEncode(cleanEmail) : string.Empty;

            var cleanInterest = SanitizeSingleLineValue(model.InterestedIn);
            bool hasInterest = !string.IsNullOrWhiteSpace(cleanInterest);
            var encodedInterest = hasInterest ? WebUtility.HtmlEncode(cleanInterest) : string.Empty;

            var cleanMessage = model.Message?.Trim() ?? string.Empty;
            bool hasMessage = !string.IsNullOrWhiteSpace(cleanMessage);
            var formattedMessageHtml = string.Empty;
            if (hasMessage)
            {
                var normalizedMsg = cleanMessage.Replace("\r\n", "\n").Replace("\r", "\n");
                var encodedMsg = WebUtility.HtmlEncode(normalizedMsg);
                formattedMessageHtml = encodedMsg.Replace("\n", "<br />");
            }

            // 2. Customer Details Section (omitted completely if no customer fields exist)
            var customerRows = new System.Collections.Generic.List<(string Label, string ValueHtml)>();
            if (hasFullName)
            {
                customerRows.Add(("Full Name", $"<span style=\"font-weight:700;color:#041A33;\">{encodedFullName}</span>"));
            }
            if (hasPhone)
            {
                customerRows.Add(("Phone Number", $"<a href=\"tel:{encodedTelHref}\" style=\"color:#062545;text-decoration:none;font-weight:600;\">{encodedPhone}</a>"));
            }
            if (hasEmail)
            {
                customerRows.Add(("Email Address", $"<a href=\"mailto:{encodedMailtoHref}\" style=\"color:#062545;text-decoration:none;font-weight:600;\">{encodedEmail}</a>"));
            }

            var customerDetailsSectionHtml = customerRows.Count > 0
                ? $@"
                            <!-- SECTION: Customer Details -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom:22px;border:1px solid #EAE5D7;border-radius:3px;"">
                                <tr>
                                    <td style=""background-color:#062545;padding:10px 18px;"">
                                        <span style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;letter-spacing:1.2px;text-transform:uppercase;color:#F5D98A;"">
                                            Customer Details
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:16px 18px;background-color:#FFFFFF;"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
{BuildKeyValueRowsHtml(customerRows)}
                                        </table>
                                    </td>
                                </tr>
                            </table>"
                : string.Empty;

            // 3. Property Interest Section (omitted completely if InterestedIn is null/empty/whitespace)
            var propertyInterestSectionHtml = hasInterest
                ? $@"
                            <!-- SECTION: Property Interest -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom:22px;border:1px solid #EAE5D7;border-radius:3px;"">
                                <tr>
                                    <td style=""background-color:#062545;padding:10px 18px;"">
                                        <span style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;letter-spacing:1.2px;text-transform:uppercase;color:#F5D98A;"">
                                            Property Interest
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:16px 18px;background-color:#FFFFFF;"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                            <tr>
                                                <td style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:13px;color:#6A7B95;width:150px;vertical-align:top;"">
                                                    Interested In
                                                </td>
                                                <td style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;font-weight:700;color:#041A33;vertical-align:top;"">
                                                    {encodedInterest}
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>"
                : string.Empty;

            // 4. Customer Message Section (omitted completely if Message is null/empty/whitespace; no placeholder)
            var customerMessageSectionHtml = hasMessage
                ? $@"
                            <!-- SECTION: Customer Message -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin-bottom:22px;border:1px solid #EAE5D7;border-radius:3px;"">
                                <tr>
                                    <td style=""background-color:#062545;padding:10px 18px;"">
                                        <span style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;letter-spacing:1.2px;text-transform:uppercase;color:#F5D98A;"">
                                            Customer Message
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:16px 18px;background-color:#F8F6F0;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;line-height:1.65;color:#041A33;"">
                                        {formattedMessageHtml}
                                    </td>
                                </tr>
                            </table>"
                : string.Empty;

            // 5. Enquiry Information Section
            var enquiryInfoRows = new System.Collections.Generic.List<(string Label, string ValueHtml)>
            {
                ("Source", "<span style=\"font-weight:600;color:#041A33;\">Agam Estates Website</span>")
            };

            bool hasValidSubmittedAt = model.SubmittedAt != default && model.SubmittedAt.Year > 1900;
            if (hasValidSubmittedAt)
            {
                var encodedSubmittedDate = WebUtility.HtmlEncode(model.SubmittedAt.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture));
                var encodedSubmittedTime = WebUtility.HtmlEncode(model.SubmittedAt.ToString("hh:mm tt", CultureInfo.InvariantCulture));
                enquiryInfoRows.Add(("Submitted On", $"<span style=\"color:#041A33;\">{encodedSubmittedDate}</span>"));
                enquiryInfoRows.Add(("Submitted At", $"<span style=\"color:#041A33;\">{encodedSubmittedTime}</span>"));
            }

            if (model.LeadId > 0)
            {
                enquiryInfoRows.Add(("Reference", $"<span style=\"font-weight:700;color:#062545;\">#{model.LeadId}</span>"));
            }

            bool hasAnyCta = hasPhone || hasEmail;
            var enquirySectionMargin = hasAnyCta ? "margin-bottom:24px;" : "margin-bottom:0;";

            var enquiryInfoSectionHtml = $@"
                            <!-- SECTION: Enquiry Information -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""{enquirySectionMargin}border:1px solid #EAE5D7;border-radius:3px;"">
                                <tr>
                                    <td style=""background-color:#062545;padding:10px 18px;"">
                                        <span style=""font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;letter-spacing:1.2px;text-transform:uppercase;color:#F5D98A;"">
                                            Enquiry Information
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding:16px 18px;background-color:#FFFFFF;"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
{BuildKeyValueRowsHtml(enquiryInfoRows)}
                                        </table>
                                    </td>
                                </tr>
                            </table>";

            // 6. Final CTA Action Area
            var callButtonHtml = hasPhone
                ? $@"<a href=""tel:{encodedTelHref}"" style=""display:block;background-color:#DDAE45;border:1px solid #DDAE45;border-radius:3px;padding:12px 14px;text-align:center;text-decoration:none;font-family:'Segoe UI',Arial,Helvetica,sans-serif;"">
                                            <span style=""display:block;font-size:12px;font-weight:700;letter-spacing:1px;text-transform:uppercase;color:#041A33;"">CALL CUSTOMER</span>
                                            <span style=""display:block;font-size:12px;font-weight:600;color:#041A33;margin-top:3px;"">{encodedPhone}</span>
                                        </a>"
                : string.Empty;

            var replyButtonHtml = hasEmail
                ? $@"<a href=""mailto:{encodedMailtoHref}"" style=""display:block;background-color:#041A33;border:1px solid #041A33;border-radius:3px;padding:12px 14px;text-align:center;text-decoration:none;font-family:'Segoe UI',Arial,Helvetica,sans-serif;"">
                                            <span style=""display:block;font-size:12px;font-weight:700;letter-spacing:1px;text-transform:uppercase;color:#FFFFFF;"">REPLY BY EMAIL</span>
                                            <span style=""display:block;font-size:12px;font-weight:500;color:#F5D98A;margin-top:3px;word-break:break-all;"">{encodedEmail}</span>
                                        </a>"
                : string.Empty;

            string ctaSectionHtml = string.Empty;
            if (hasPhone && hasEmail)
            {
                ctaSectionHtml = $@"
                            <!-- FINAL ACTIONS: Call Customer & Reply by Email -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td class=""action-col"" width=""50%"" valign=""top"" style=""padding-right:8px;"">
                                        {callButtonHtml}
                                    </td>
                                    <td class=""action-col"" width=""50%"" valign=""top"" style=""padding-left:8px;"">
                                        {replyButtonHtml}
                                    </td>
                                </tr>
                            </table>";
            }
            else if (hasPhone)
            {
                ctaSectionHtml = $@"
                            <!-- FINAL ACTION: Call Customer -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td width=""100%"" valign=""top"">
                                        {callButtonHtml}
                                    </td>
                                </tr>
                            </table>";
            }
            else if (hasEmail)
            {
                ctaSectionHtml = $@"
                            <!-- FINAL ACTION: Reply by Email -->
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td width=""100%"" valign=""top"">
                                        {replyButtonHtml}
                                    </td>
                                </tr>
                            </table>";
            }

            return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <title>{encodedDocumentTitle}</title>
    <!--[if mso]>
    <style type=""text/css"">
        table {{ border-collapse: collapse; border-spacing: 0; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
        td, th {{ font-family: Arial, sans-serif; }}
    </style>
    <![endif]-->
    <style type=""text/css"">
        @media only screen and (max-width: 620px) {{
            .email-container {{
                width: 100% !important;
                max-width: 100% !important;
            }}
            .email-pad {{
                padding-left: 20px !important;
                padding-right: 20px !important;
            }}
            .action-col {{
                display: block !important;
                width: 100% !important;
                padding-left: 0 !important;
                padding-right: 0 !important;
                padding-bottom: 12px !important;
            }}
        }}
    </style>
</head>
<body style=""margin:0;padding:0;background-color:#F8F6F0;font-family:'Segoe UI',Arial,Helvetica,sans-serif;-webkit-font-smoothing:antialiased;"">
    <!-- Outer Background Table -->
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color:#F8F6F0;width:100%;margin:0;padding:28px 12px;"">
        <tr>
            <td align=""center"" valign=""top"">
                <!-- Main Email Container (620px) -->
                <table role=""presentation"" class=""email-container"" width=""620"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""width:100%;max-width:620px;background-color:#FFFFFF;border:1px solid #E4DEC9;border-radius:4px;overflow:hidden;box-shadow:0 6px 24px rgba(4,26,51,0.06);"">
                    
                    <!-- HEADER: Dark Navy Background -->
                    <tr>
                        <td class=""email-pad"" style=""background-color:#041A33;padding:28px 36px;border-bottom:3px solid #DDAE45;text-align:left;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                                <tr>
                                    <td valign=""middle"">
                                        <div style=""font-family:Georgia,'Times New Roman',serif;font-size:22px;font-weight:700;letter-spacing:2.5px;color:#FFFFFF;text-transform:uppercase;line-height:1.2;"">
                                            AGAM <span style=""color:#DDAE45;"">ESTATES</span>
                                        </div>
                                    </td>
                                    <td align=""right"" valign=""middle"">
                                        <span style=""display:inline-block;background-color:rgba(221,174,69,0.16);border:1px solid #DDAE45;color:#F5D98A;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:11px;font-weight:700;letter-spacing:1.4px;text-transform:uppercase;padding:6px 12px;border-radius:2px;"">
                                            NEW LEAD RECEIVED
                                        </span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- BODY -->
                    <tr>
                        <td class=""email-pad"" style=""padding:32px 36px;background-color:#FFFFFF;"">
                            
                            <!-- Greeting & Intro -->
                            <p style=""margin:0 0 10px 0;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:15px;font-weight:600;color:#062545;"">
                                Hello Team,
                            </p>
                            <p style=""margin:0 0 24px 0;font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;line-height:1.65;color:#334760;"">
                                A new customer has submitted an enquiry through the Agam Estates website.
                            </p>{customerDetailsSectionHtml}{propertyInterestSectionHtml}{customerMessageSectionHtml}{enquiryInfoSectionHtml}{ctaSectionHtml}

                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private static string BuildKeyValueRowsHtml(System.Collections.Generic.IReadOnlyList<(string Label, string ValueHtml)> rows)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < rows.Count; i++)
            {
                bool isFirst = i == 0;
                bool isLast = i == rows.Count - 1;

                var topPadding = isFirst ? "0" : "10px";
                var bottomPadding = isLast ? "0" : "10px";
                var borderTop = isFirst ? string.Empty : "border-top:1px solid #EFECE4;";

                sb.Append($@"                                            <tr>
                                                <td style=""padding:{topPadding} 0 {bottomPadding} 0;{borderTop}font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:13px;color:#6A7B95;width:150px;vertical-align:top;"">
                                                    {rows[i].Label}
                                                </td>
                                                <td style=""padding:{topPadding} 0 {bottomPadding} 0;{borderTop}font-family:'Segoe UI',Arial,Helvetica,sans-serif;font-size:14px;color:#041A33;vertical-align:top;"">
                                                    {rows[i].ValueHtml}
                                                </td>
                                            </tr>");
                if (!isLast)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
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

        private static string SanitizeHeaderValue(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Strip CR/LF and control characters to prevent SMTP header injection
            var cleaned = Regex.Replace(input, @"[\r\n\0]+", " ").Trim();
            return cleaned;
        }

        private static string SanitizeSingleLineValue(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var cleaned = Regex.Replace(input, @"[\r\n\0]+", " ").Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? string.Empty : cleaned;
        }

        private static string NormalizeAppPassword(string? rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                return string.Empty;
            }

            // Google App Passwords are 16 chars often displayed with spaces (xxxx xxxx xxxx xxxx)
            return Regex.Replace(rawPassword, @"\s+", string.Empty);
        }
    }
}

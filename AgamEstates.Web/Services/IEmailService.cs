using System;
using System.Threading.Tasks;

namespace AgamEstates.Web.Services
{
    public class EnquiryEmailNotificationModel
    {
        public int LeadId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string InterestedIn { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public interface IEmailService
    {
        Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody,
            string? replyToEmail = null,
            string? replyToName = null);

        Task SendTestConnectionEmailAsync(ResolvedSmtpConfiguration config);

        string BuildEnquirySubject(string? fullName);

        string BuildEnquiryNotificationHtml(EnquiryEmailNotificationModel model);
    }
}

using System.ComponentModel.DataAnnotations;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

public class LeadSubmitInputModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Message { get; set; }

    public string? Website { get; set; }
}

public class DashboardViewModel
{
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int ReadLeads { get; set; }
    public int TodaysLeads { get; set; }
    public List<Lead> RecentLeads { get; set; } = new();
}

public class LeadsListViewModel
{
    public List<Lead> Leads { get; set; } = new();
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(PageSize, 1));
    public int AllCount { get; set; }
    public int NewCount { get; set; }
    public int ReadCount { get; set; }
}

public class UsersListViewModel
{
    public List<User> Users { get; set; } = new();
    public int CurrentUserId { get; set; }
}

public class CreateUserInputModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = "Agent";

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class EditUserInputModel
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = "Agent";

    public bool IsActive { get; set; } = true;
}

public class ResetUserPasswordInputModel
{
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SystemEmailSettingsViewModel
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string? FromName { get; set; } = "VS Moving Services";
    public string? FromEmail { get; set; }
    public string ReceiverEmail { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public bool HasStoredPassword { get; set; }
    public string ConnectionStatus { get; set; } = "Not Tested";
    public string? LastTestedAtFormatted { get; set; }
    public bool? LastTestSucceeded { get; set; }
    public string? LastTestMessage { get; set; }

    // Website & Social Settings
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? WebsiteEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? BusinessAddress { get; set; }
}

public class WebsiteSettingsViewModel
{
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? WebsiteEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? BusinessAddress { get; set; }
    public List<PublicTestimonialViewModel> Testimonials { get; set; } = new();

    public string CleanPrimaryPhoneTel => CleanPhoneForTel(PrimaryPhone);
    public string CleanSecondaryPhoneTel => CleanPhoneForTel(SecondaryPhone);
    public string CleanWhatsAppNumber => CleanNumberDigitsOnly(WhatsAppNumber);
    public string WhatsAppUrl => HasWhatsApp ? $"https://wa.me/{CleanWhatsAppNumber}" : string.Empty;

    public bool HasFacebook => !string.IsNullOrWhiteSpace(FacebookUrl);
    public bool HasInstagram => !string.IsNullOrWhiteSpace(InstagramUrl);
    public bool HasWhatsApp => !string.IsNullOrWhiteSpace(CleanWhatsAppNumber);
    public bool HasAnySocial => HasFacebook || HasInstagram || HasWhatsApp;
    public bool HasWebsiteEmail => !string.IsNullOrWhiteSpace(WebsiteEmail);
    public bool HasPrimaryPhone => !string.IsNullOrWhiteSpace(PrimaryPhone) && !string.IsNullOrWhiteSpace(CleanPrimaryPhoneTel);
    public bool HasSecondaryPhone => !string.IsNullOrWhiteSpace(SecondaryPhone) && !string.IsNullOrWhiteSpace(CleanSecondaryPhoneTel);
    public bool HasBusinessAddress => !string.IsNullOrWhiteSpace(BusinessAddress);

    public static string CleanPhoneForTel(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var trimmed = raw.Trim();
        var chars = trimmed.Where((c, i) => char.IsDigit(c) || (c == '+' && i == 0)).ToArray();
        return new string(chars);
    }

    public static string CleanNumberDigitsOnly(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var digits = raw.Where(char.IsDigit).ToArray();
        return new string(digits);
    }
}

public class PublicTestimonialViewModel
{
    public int TestimonialId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public int DisplayOrder { get; set; } = 1;
}

public class TestimonialsListViewModel
{
    public List<Testimonial> Testimonials { get; set; } = new();
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public int TotalCount { get; set; }
    public int PublishedCount { get; set; }
    public int UnpublishedCount { get; set; }
    public int NextDisplayOrder { get; set; } = 1;
}

public class SaveTestimonialInputModel
{
    [Required(ErrorMessage = "Customer Name is required.")]
    [MaxLength(120, ErrorMessage = "Customer Name cannot exceed 120 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(120, ErrorMessage = "Location cannot exceed 120 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Review Text is required.")]
    [MaxLength(1000, ErrorMessage = "Review Text cannot exceed 1000 characters.")]
    public string ReviewText { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    public int Rating { get; set; } = 5;

    public int DisplayOrder { get; set; } = 1;

    public bool IsPublished { get; set; } = true;
}

public class EditTestimonialInputModel
{
    [Required]
    public int TestimonialId { get; set; }

    [Required(ErrorMessage = "Customer Name is required.")]
    [MaxLength(120, ErrorMessage = "Customer Name cannot exceed 120 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(120, ErrorMessage = "Location cannot exceed 120 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Review Text is required.")]
    [MaxLength(1000, ErrorMessage = "Review Text cannot exceed 1000 characters.")]
    public string ReviewText { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    public int Rating { get; set; } = 5;

    public int DisplayOrder { get; set; } = 1;

    public bool IsPublished { get; set; } = true;
}

public class SaveWebsiteSettingsInputModel
{
    [MaxLength(300, ErrorMessage = "Facebook URL cannot exceed 300 characters.")]
    public string? FacebookUrl { get; set; }

    [MaxLength(300, ErrorMessage = "Instagram URL cannot exceed 300 characters.")]
    public string? InstagramUrl { get; set; }

    [MaxLength(40, ErrorMessage = "WhatsApp Number cannot exceed 40 characters.")]
    public string? WhatsAppNumber { get; set; }

    [MaxLength(150, ErrorMessage = "Website Email cannot exceed 150 characters.")]
    public string? WebsiteEmail { get; set; }

    [MaxLength(40, ErrorMessage = "Primary Phone cannot exceed 40 characters.")]
    public string? PrimaryPhone { get; set; }

    [MaxLength(40, ErrorMessage = "Secondary Phone cannot exceed 40 characters.")]
    public string? SecondaryPhone { get; set; }

    [MaxLength(250, ErrorMessage = "Business Address cannot exceed 250 characters.")]
    public string? BusinessAddress { get; set; }
}

public class SaveEmailSettingsInputModel
{
    [Required(ErrorMessage = "SMTP Host is required.")]
    [MaxLength(200)]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP Port must be between 1 and 65535.")]
    public int SmtpPort { get; set; } = 587;

    [Required(ErrorMessage = "Sender Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid Sender Email address.")]
    [MaxLength(150)]
    public string SenderEmail { get; set; } = string.Empty;

    public string? SmtpPassword { get; set; }

    [MaxLength(120)]
    public string? FromName { get; set; }

    [MaxLength(150)]
    public string? FromEmail { get; set; }

    [Required(ErrorMessage = "Receiver Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid Receiver Email address.")]
    [MaxLength(150)]
    public string ReceiverEmail { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;
}

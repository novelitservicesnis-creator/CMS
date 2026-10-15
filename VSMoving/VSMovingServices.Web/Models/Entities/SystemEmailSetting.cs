using System.ComponentModel.DataAnnotations;

namespace VSMovingServices.Web.Models.Entities;

public class SystemEmailSetting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    public int SmtpPort { get; set; } = 587;

    [Required]
    [MaxLength(150)]
    public string SenderEmail { get; set; } = string.Empty;

    public string? EncryptedSmtpPassword { get; set; }

    [MaxLength(120)]
    public string? FromName { get; set; } = "VS Moving Services";

    [MaxLength(150)]
    public string? FromEmail { get; set; }

    [Required]
    [MaxLength(150)]
    public string ReceiverEmail { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    public bool ConfigurationChanged { get; set; } = false;

    public DateTime? LastTestedAt { get; set; }

    public bool? LastTestSucceeded { get; set; }

    [MaxLength(500)]
    public string? LastTestMessage { get; set; }

    [MaxLength(300)]
    public string? FacebookUrl { get; set; }

    [MaxLength(300)]
    public string? InstagramUrl { get; set; }

    [MaxLength(40)]
    public string? WhatsAppNumber { get; set; } = "15149999790";

    [MaxLength(150)]
    public string? WebsiteEmail { get; set; } = "vsshippingservicespvt@gmail.com";

    [MaxLength(40)]
    public string? PrimaryPhone { get; set; } = "514 999 9790";

    [MaxLength(40)]
    public string? SecondaryPhone { get; set; } = "438 990 8055";

    [MaxLength(250)]
    public string? BusinessAddress { get; set; } = "Halifax, Nova Scotia";

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

using System.ComponentModel.DataAnnotations;

namespace VSMovingServices.Web.Models.Entities;

public class Lead
{
    [Key]
    public int LeadId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PhoneNumber { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(1000)]
    public string? Message { get; set; }

    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = "VS Moving Services Website";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}

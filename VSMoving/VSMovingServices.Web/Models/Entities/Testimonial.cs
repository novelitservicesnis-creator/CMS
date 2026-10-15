using System.ComponentModel.DataAnnotations;

namespace VSMovingServices.Web.Models.Entities;

public class Testimonial
{
    [Key]
    public int TestimonialId { get; set; }

    [Required]
    [MaxLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Location { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ReviewText { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    public int DisplayOrder { get; set; } = 1;

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }
}

using System;

namespace AgamEstates.Core.Models
{
    public class BlogPost
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public int? BlogCategoryId { get; set; }
        public int? AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public virtual BlogCategory? Category { get; set; }
        public virtual User? Author { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace AgamEstates.Core.Models
{
    public class BlogCategory
    {
        public BlogCategory()
        {
            BlogPosts = new HashSet<BlogPost>();
        }

        public int BlogCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set; }
    }
}

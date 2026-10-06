using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgamEstates.Repository.ViewModel
{
    public class BlogCategoryDto
    {
        public int BlogCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalBlogsCount { get; set; }
        public int PublishedBlogsCount { get; set; }
    }

    public class BlogPostDto
    {
        public int BlogPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImage { get; set; }
        public int? BlogCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public int? AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class CreateUpdateBlogPostDto
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "Blog title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(220, ErrorMessage = "Slug cannot exceed 220 characters.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Short description is required.")]
        [StringLength(600, ErrorMessage = "Short description cannot exceed 600 characters.")]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Article content is required.")]
        public string Content { get; set; } = string.Empty;

        public string? FeaturedImage { get; set; }

        [Required(ErrorMessage = "Please select a blog category.")]
        public int? BlogCategoryId { get; set; }

        public int? AuthorUserId { get; set; }

        [StringLength(150, ErrorMessage = "Author name cannot exceed 150 characters.")]
        public string? AuthorName { get; set; }

        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? AdminUserId { get; set; }
    }

    public class CreateUpdateBlogCategoryDto
    {
        public int BlogCategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(120, ErrorMessage = "Slug cannot exceed 120 characters.")]
        public string? Slug { get; set; }

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class BlogSummaryCountsDto
    {
        public int TotalBlogs { get; set; }
        public int PublishedBlogs { get; set; }
        public int DraftBlogs { get; set; }
        public int ActiveCategories { get; set; }
    }

    public class PublicBlogListingDto
    {
        public BlogPostDto? FeaturedPost { get; set; }
        public List<BlogPostDto> Posts { get; set; } = new();
        public List<BlogCategoryDto> Categories { get; set; } = new();
        public string? ActiveCategorySlug { get; set; }
        public string? ActiveCategoryName { get; set; }
        public string? SearchQuery { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalPosts { get; set; }
        public int TotalPages { get; set; }
    }
}

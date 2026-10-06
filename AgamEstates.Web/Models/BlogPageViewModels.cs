using AgamEstates.Repository.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgamEstates.Web.Models
{
    public class AdminBlogsPageViewModel
    {
        public BlogSummaryCountsDto Summary { get; set; } = new();
        public List<BlogPostDto> Blogs { get; set; } = new();
        public List<BlogCategoryDto> Categories { get; set; } = new();
        public string? Search { get; set; }
        public int? CategoryFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class BlogFormViewModel
    {
        public int BlogPostId { get; set; }

        [Required(ErrorMessage = "Blog title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(220, ErrorMessage = "Slug cannot exceed 220 characters.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int? BlogCategoryId { get; set; }

        [Required(ErrorMessage = "Short description is required.")]
        [StringLength(600, MinimumLength = 20, ErrorMessage = "Short description must be between 20 and 600 characters.")]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Blog content is required.")]
        public string Content { get; set; } = string.Empty;

        public string? ExistingFeaturedImage { get; set; }
        public IFormFile? FeaturedImageFile { get; set; }

        public string PublishStatus { get; set; } = "Draft";
        public DateTime? PublishedAt { get; set; }

        public int? AuthorUserId { get; set; }

        [StringLength(150, ErrorMessage = "Author name cannot exceed 150 characters.")]
        public string? AuthorName { get; set; }

        public string DefaultAdminAuthorName { get; set; } = "Agam Estates Editorial Team";

        public List<BlogCategoryDto> Categories { get; set; } = new();
        public bool IsEditMode { get; set; }
    }

    public class AdminBlogCategoriesPageViewModel
    {
        public List<BlogCategoryDto> Categories { get; set; } = new();
        public int TotalCategories => Categories.Count;
        public int ActiveCategories => Categories.FindAll(c => c.IsActive).Count;
        public int InactiveCategories => Categories.FindAll(c => !c.IsActive).Count;
    }

    public class PublicBlogDetailViewModel
    {
        public BlogPostDto Post { get; set; } = new();
        public List<BlogCategoryDto> Categories { get; set; } = new();
        public List<BlogPostDto> RecentPosts { get; set; } = new();
        public bool IsPreviewMode { get; set; }
    }
}

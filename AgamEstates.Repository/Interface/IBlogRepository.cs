using AgamEstates.Repository.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Interface
{
    public interface IBlogRepository
    {
        // Helpers
        string GenerateSlug(string input, int maxLength = 200);
        string SanitizeHtml(string? html);
        Task<bool> IsBlogSlugUniqueAsync(string slug, int? excludeBlogPostId = null);
        Task<bool> IsCategorySlugUniqueAsync(string slug, int? excludeCategoryId = null);

        // Admin Blog Management
        Task<BlogSummaryCountsDto> GetSummaryCountsAsync();
        Task<List<BlogPostDto>> GetAdminBlogsAsync(string? search = null, int? categoryId = null, string? status = null);
        Task<BlogPostDto?> GetBlogByIdAsync(int id);
        Task<(bool Success, string Message, int BlogPostId)> CreateBlogAsync(CreateUpdateBlogPostDto dto);
        Task<(bool Success, string Message)> UpdateBlogAsync(CreateUpdateBlogPostDto dto);
        Task<(bool Success, string Message, string? DeletedFeaturedImage)> DeleteBlogAsync(int id);

        // Admin Category Management
        Task<List<BlogCategoryDto>> GetAllCategoriesAsync(bool activeOnly = false);
        Task<BlogCategoryDto?> GetCategoryByIdAsync(int id);
        Task<(bool Success, string Message, int CategoryId)> CreateCategoryAsync(CreateUpdateBlogCategoryDto dto);
        Task<(bool Success, string Message)> UpdateCategoryAsync(CreateUpdateBlogCategoryDto dto);
        Task<(bool Success, string Message)> ToggleCategoryStatusAsync(int id);
        Task<(bool Success, string Message)> DeleteCategoryAsync(int id);

        // Public Blog Queries (Read-only, AsNoTracking)
        Task<PublicBlogListingDto> GetPublicBlogListingAsync(string? categorySlug = null, string? search = null, int page = 1, int pageSize = 9);
        Task<BlogPostDto?> GetPublicBlogBySlugAsync(string slug);
        Task<List<BlogPostDto>> GetRecentPublishedBlogsAsync(int count = 4, int? excludeBlogPostId = null);
        Task<List<BlogCategoryDto>> GetPublicCategoriesAsync();
    }
}

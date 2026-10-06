using AgamEstates.Core;
using AgamEstates.Core.Models;
using AgamEstates.Repository.Base;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgamEstates.Repository.Repository
{
    public class BlogRepository : RepositoryBase<AgamEntities>, IBlogRepository
    {
        public BlogRepository(AgamEntities dataContext) : base(dataContext)
        {
        }

        public string GenerateSlug(string input, int maxLength = 200)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var slug = input.Trim().ToLowerInvariant();
            slug = slug.Replace("&", " and ");
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

            if (slug.Length > maxLength)
            {
                slug = slug.Substring(0, maxLength).TrimEnd('-');
            }

            return slug;
        }

        public string SanitizeHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var cleaned = html.Trim();

            // Remove dangerous paired tags and their inner content
            cleaned = Regex.Replace(
                cleaned,
                @"<\s*(script|iframe|object|embed|form|style|svg|math|applet|frame|frameset|NOSCRIPT)\b[^>]*>[\s\S]*?<\s*/\s*\1\s*>",
                string.Empty,
                RegexOptions.IgnoreCase);

            // Remove dangerous self-closing or unclosed tags
            cleaned = Regex.Replace(
                cleaned,
                @"<\s*/?\s*(script|iframe|object|embed|form|input|button|select|textarea|style|link|meta|base|applet|frame|frameset)\b[^>]*>",
                string.Empty,
                RegexOptions.IgnoreCase);

            // Strip inline event handlers (onclick, onerror, onload, etc.)
            cleaned = Regex.Replace(
                cleaned,
                @"\s+on[a-z]+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)",
                string.Empty,
                RegexOptions.IgnoreCase);

            // Strip inline style attributes that could contain expressions or layout-breaking widths
            cleaned = Regex.Replace(
                cleaned,
                @"\s+style\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)",
                string.Empty,
                RegexOptions.IgnoreCase);

            // Strip javascript:, vbscript:, and data: protocols in href/src/action attributes
            cleaned = Regex.Replace(
                cleaned,
                @"(href|src|action|xlink:href)\s*=\s*([""']?)\s*(javascript|vbscript|data)\s*:[^""'>\s]*\2",
                "$1=\"#\"",
                RegexOptions.IgnoreCase);

            // Ensure links with target="_blank" include rel="noopener noreferrer"
            cleaned = Regex.Replace(
                cleaned,
                @"<a\b([^>]*\btarget\s*=\s*[""']?_blank[""']?[^>]*)>",
                match =>
                {
                    var attrs = match.Groups[1].Value;
                    if (Regex.IsMatch(attrs, @"\brel\s*=", RegexOptions.IgnoreCase))
                    {
                        return match.Value;
                    }
                    return $"<a{attrs} rel=\"noopener noreferrer\">";
                },
                RegexOptions.IgnoreCase);

            return cleaned;
        }

        private static bool HasMeaningfulContent(string? sanitizedHtml)
        {
            if (string.IsNullOrWhiteSpace(sanitizedHtml))
            {
                return false;
            }

            var textOnly = Regex.Replace(sanitizedHtml, @"<[^>]+>", " ");
            textOnly = Regex.Replace(textOnly, @"(&nbsp;|&#160;|&#xa0;)", " ", RegexOptions.IgnoreCase);
            textOnly = System.Net.WebUtility.HtmlDecode(textOnly);
            return !string.IsNullOrWhiteSpace(textOnly);
        }

        public async Task<bool> IsBlogSlugUniqueAsync(string slug, int? excludeBlogPostId = null)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;
            var normalized = slug.Trim().ToLowerInvariant();

            var query = DataContext.BlogPosts.AsNoTracking().Where(b => b.Slug.ToLower() == normalized);
            if (excludeBlogPostId.HasValue && excludeBlogPostId.Value > 0)
            {
                query = query.Where(b => b.BlogPostId != excludeBlogPostId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsCategorySlugUniqueAsync(string slug, int? excludeCategoryId = null)
        {
            if (string.IsNullOrWhiteSpace(slug)) return false;
            var normalized = slug.Trim().ToLowerInvariant();

            var query = DataContext.BlogCategories.AsNoTracking().Where(c => c.Slug.ToLower() == normalized);
            if (excludeCategoryId.HasValue && excludeCategoryId.Value > 0)
            {
                query = query.Where(c => c.BlogCategoryId != excludeCategoryId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<BlogSummaryCountsDto> GetSummaryCountsAsync()
        {
            var total = await DataContext.BlogPosts.AsNoTracking().CountAsync();
            var published = await DataContext.BlogPosts.AsNoTracking().CountAsync(b => b.IsPublished);
            var drafts = total - published;
            var activeCategories = await DataContext.BlogCategories.AsNoTracking().CountAsync(c => c.IsActive);

            return new BlogSummaryCountsDto
            {
                TotalBlogs = total,
                PublishedBlogs = published,
                DraftBlogs = drafts,
                ActiveCategories = activeCategories
            };
        }

        public async Task<List<BlogPostDto>> GetAdminBlogsAsync(string? search = null, int? categoryId = null, string? status = null)
        {
            var query = DataContext.BlogPosts
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(b =>
                    b.Title.ToLower().Contains(term) ||
                    b.Slug.ToLower().Contains(term) ||
                    (b.ShortDescription != null && b.ShortDescription.ToLower().Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.BlogCategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Published", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(b => b.IsPublished);
                }
                else if (status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(b => !b.IsPublished);
                }
            }

            var posts = await query
                .OrderByDescending(b => b.CreatedAt)
                .ThenByDescending(b => b.BlogPostId)
                .Select(b => new BlogPostDto
                {
                    BlogPostId = b.BlogPostId,
                    Title = b.Title,
                    Slug = b.Slug,
                    ShortDescription = b.ShortDescription,
                    Content = b.Content,
                    FeaturedImage = b.FeaturedImage,
                    BlogCategoryId = b.BlogCategoryId,
                    CategoryName = b.Category != null ? b.Category.CategoryName : "Uncategorized",
                    CategorySlug = b.Category != null ? b.Category.Slug : null,
                    AuthorUserId = b.AuthorUserId,
                    AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                        ? b.AuthorName.Trim()
                        : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    CreatedBy = b.CreatedBy,
                    UpdatedBy = b.UpdatedBy
                })
                .ToListAsync();

            return posts;
        }

        public async Task<BlogPostDto?> GetBlogByIdAsync(int id)
        {
            var b = await DataContext.BlogPosts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.BlogPostId == id);

            if (b == null) return null;

            return new BlogPostDto
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                Content = b.Content,
                FeaturedImage = b.FeaturedImage,
                BlogCategoryId = b.BlogCategoryId,
                CategoryName = b.Category != null ? b.Category.CategoryName : "Uncategorized",
                CategorySlug = b.Category != null ? b.Category.Slug : null,
                AuthorUserId = b.AuthorUserId,
                AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                    ? b.AuthorName.Trim()
                    : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                IsPublished = b.IsPublished,
                PublishedAt = b.PublishedAt,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                CreatedBy = b.CreatedBy,
                UpdatedBy = b.UpdatedBy
            };
        }

        public async Task<(bool Success, string Message, int BlogPostId)> CreateBlogAsync(CreateUpdateBlogPostDto dto)
        {
            var title = (dto.Title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Blog title is required.", 0);
            }

            var rawSlug = !string.IsNullOrWhiteSpace(dto.Slug) ? dto.Slug! : title;
            var slug = GenerateSlug(rawSlug, 200);
            if (string.IsNullOrWhiteSpace(slug))
            {
                return (false, "Unable to generate a valid URL slug from the provided title or slug.", 0);
            }

            if (!await IsBlogSlugUniqueAsync(slug))
            {
                return (false, $"The URL slug '{slug}' is already in use by another blog post. Please enter a unique slug.", 0);
            }

            var sanitizedContent = SanitizeHtml(dto.Content);
            if (!HasMeaningfulContent(sanitizedContent))
            {
                return (false, "Blog content cannot be empty.", 0);
            }

            var authorName = !string.IsNullOrWhiteSpace(dto.AuthorName) ? dto.AuthorName.Trim() : null;
            if (authorName != null && authorName.Length > 150)
            {
                return (false, "Author name cannot exceed 150 characters.", 0);
            }

            DateTime? publishedAt = null;
            if (dto.IsPublished)
            {
                publishedAt = dto.PublishedAt ?? DateTime.UtcNow;
            }
            else if (dto.PublishedAt.HasValue)
            {
                publishedAt = dto.PublishedAt.Value;
            }

            var entity = new BlogPost
            {
                Title = title,
                Slug = slug,
                ShortDescription = dto.ShortDescription?.Trim(),
                Content = sanitizedContent,
                FeaturedImage = dto.FeaturedImage,
                BlogCategoryId = dto.BlogCategoryId,
                AuthorUserId = dto.AuthorUserId ?? dto.AdminUserId,
                AuthorName = authorName,
                IsPublished = dto.IsPublished,
                PublishedAt = publishedAt,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.AdminUserId
            };

            DataContext.BlogPosts.Add(entity);
            await DataContext.SaveChangesAsync();

            return (true, dto.IsPublished ? "Blog published successfully." : "Draft saved successfully.", entity.BlogPostId);
        }

        public async Task<(bool Success, string Message)> UpdateBlogAsync(CreateUpdateBlogPostDto dto)
        {
            var entity = await DataContext.BlogPosts.FirstOrDefaultAsync(b => b.BlogPostId == dto.BlogPostId);
            if (entity == null)
            {
                return (false, "Blog article not found.");
            }

            var title = (dto.Title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                return (false, "Blog title is required.");
            }

            var rawSlug = !string.IsNullOrWhiteSpace(dto.Slug) ? dto.Slug! : title;
            var slug = GenerateSlug(rawSlug, 200);
            if (string.IsNullOrWhiteSpace(slug))
            {
                return (false, "Unable to generate a valid URL slug.");
            }

            if (!await IsBlogSlugUniqueAsync(slug, entity.BlogPostId))
            {
                return (false, $"The URL slug '{slug}' is already in use by another blog post. Please choose a different slug.");
            }

            var sanitizedContent = SanitizeHtml(dto.Content);
            if (!HasMeaningfulContent(sanitizedContent))
            {
                return (false, "Blog content cannot be empty.");
            }

            var authorName = !string.IsNullOrWhiteSpace(dto.AuthorName) ? dto.AuthorName.Trim() : null;
            if (authorName != null && authorName.Length > 150)
            {
                return (false, "Author name cannot exceed 150 characters.");
            }

            entity.Title = title;
            entity.Slug = slug;
            entity.ShortDescription = dto.ShortDescription?.Trim();
            entity.Content = sanitizedContent;
            entity.BlogCategoryId = dto.BlogCategoryId;
            entity.AuthorUserId = entity.AuthorUserId ?? dto.AuthorUserId ?? dto.AdminUserId;
            entity.AuthorName = authorName;

            if (!string.IsNullOrWhiteSpace(dto.FeaturedImage))
            {
                entity.FeaturedImage = dto.FeaturedImage;
            }

            if (dto.IsPublished)
            {
                entity.IsPublished = true;
                entity.PublishedAt = dto.PublishedAt ?? entity.PublishedAt ?? DateTime.UtcNow;
            }
            else
            {
                entity.IsPublished = false;
                entity.PublishedAt = dto.PublishedAt;
            }

            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = dto.AdminUserId;

            await DataContext.SaveChangesAsync();
            return (true, entity.IsPublished ? "Blog published successfully." : "Draft saved successfully.");
        }

        public async Task<(bool Success, string Message, string? DeletedFeaturedImage)> DeleteBlogAsync(int id)
        {
            var entity = await DataContext.BlogPosts.FirstOrDefaultAsync(b => b.BlogPostId == id);
            if (entity == null)
            {
                return (false, "Blog article not found.", null);
            }

            var oldImage = entity.FeaturedImage;
            DataContext.BlogPosts.Remove(entity);
            await DataContext.SaveChangesAsync();

            return (true, "Blog article deleted permanently.", oldImage);
        }

        public async Task<List<BlogCategoryDto>> GetAllCategoriesAsync(bool activeOnly = false)
        {
            var query = DataContext.BlogCategories.AsNoTracking().AsQueryable();
            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new BlogCategoryDto
                {
                    BlogCategoryId = c.BlogCategoryId,
                    CategoryName = c.CategoryName,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    TotalBlogsCount = c.BlogPosts.Count(),
                    PublishedBlogsCount = c.BlogPosts.Count(p => p.IsPublished)
                })
                .ToListAsync();

            return categories;
        }

        public async Task<BlogCategoryDto?> GetCategoryByIdAsync(int id)
        {
            var c = await DataContext.BlogCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BlogCategoryId == id);

            if (c == null) return null;

            var totalCount = await DataContext.BlogPosts.AsNoTracking().CountAsync(p => p.BlogCategoryId == id);
            var pubCount = await DataContext.BlogPosts.AsNoTracking().CountAsync(p => p.BlogCategoryId == id && p.IsPublished);

            return new BlogCategoryDto
            {
                BlogCategoryId = c.BlogCategoryId,
                CategoryName = c.CategoryName,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                TotalBlogsCount = totalCount,
                PublishedBlogsCount = pubCount
            };
        }

        public async Task<(bool Success, string Message, int CategoryId)> CreateCategoryAsync(CreateUpdateBlogCategoryDto dto)
        {
            var name = (dto.CategoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Category name is required.", 0);
            }

            var rawSlug = !string.IsNullOrWhiteSpace(dto.Slug) ? dto.Slug! : name;
            var slug = GenerateSlug(rawSlug, 110);
            if (string.IsNullOrWhiteSpace(slug))
            {
                return (false, "Unable to generate a valid category slug.", 0);
            }

            if (!await IsCategorySlugUniqueAsync(slug))
            {
                return (false, $"Category slug '{slug}' already exists. Please use a unique name or slug.", 0);
            }

            var entity = new BlogCategory
            {
                CategoryName = name,
                Slug = slug,
                Description = dto.Description?.Trim(),
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            DataContext.BlogCategories.Add(entity);
            await DataContext.SaveChangesAsync();

            return (true, "Category created successfully.", entity.BlogCategoryId);
        }

        public async Task<(bool Success, string Message)> UpdateCategoryAsync(CreateUpdateBlogCategoryDto dto)
        {
            var entity = await DataContext.BlogCategories.FirstOrDefaultAsync(c => c.BlogCategoryId == dto.BlogCategoryId);
            if (entity == null)
            {
                return (false, "Category not found.");
            }

            var name = (dto.CategoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Category name is required.");
            }

            var rawSlug = !string.IsNullOrWhiteSpace(dto.Slug) ? dto.Slug! : name;
            var slug = GenerateSlug(rawSlug, 110);
            if (string.IsNullOrWhiteSpace(slug))
            {
                return (false, "Unable to generate a valid category slug.");
            }

            if (!await IsCategorySlugUniqueAsync(slug, entity.BlogCategoryId))
            {
                return (false, $"Category slug '{slug}' is already used by another category.");
            }

            entity.CategoryName = name;
            entity.Slug = slug;
            entity.Description = dto.Description?.Trim();
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            await DataContext.SaveChangesAsync();
            return (true, "Category updated successfully.");
        }

        public async Task<(bool Success, string Message)> ToggleCategoryStatusAsync(int id)
        {
            var entity = await DataContext.BlogCategories.FirstOrDefaultAsync(c => c.BlogCategoryId == id);
            if (entity == null)
            {
                return (false, "Category not found.");
            }

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await DataContext.SaveChangesAsync();

            return (true, entity.IsActive ? $"Category '{entity.CategoryName}' activated." : $"Category '{entity.CategoryName}' deactivated.");
        }

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id)
        {
            var entity = await DataContext.BlogCategories.FirstOrDefaultAsync(c => c.BlogCategoryId == id);
            if (entity == null)
            {
                return (false, "Category not found.");
            }

            var blogCount = await DataContext.BlogPosts.AsNoTracking().CountAsync(p => p.BlogCategoryId == id);
            if (blogCount > 0)
            {
                return (false, $"Cannot delete '{entity.CategoryName}' because {blogCount} blog article(s) are assigned to it. Please deactivate the category or reassign its articles first.");
            }

            DataContext.BlogCategories.Remove(entity);
            await DataContext.SaveChangesAsync();
            return (true, $"Category '{entity.CategoryName}' deleted.");
        }

        public async Task<PublicBlogListingDto> GetPublicBlogListingAsync(string? categorySlug = null, string? search = null, int page = 1, int pageSize = 9)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 9;

            var categories = await GetPublicCategoriesAsync();

            var baseQuery = DataContext.BlogPosts
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Where(b => b.IsPublished && (b.Category == null || b.Category.IsActive));

            string? activeCategoryName = null;
            if (!string.IsNullOrWhiteSpace(categorySlug))
            {
                var slugNorm = categorySlug.Trim().ToLowerInvariant();
                baseQuery = baseQuery.Where(b => b.Category != null && b.Category.Slug.ToLower() == slugNorm);
                activeCategoryName = categories.FirstOrDefault(c => c.Slug.Equals(slugNorm, StringComparison.OrdinalIgnoreCase))?.CategoryName;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                baseQuery = baseQuery.Where(b =>
                    b.Title.ToLower().Contains(term) ||
                    (b.ShortDescription != null && b.ShortDescription.ToLower().Contains(term)) ||
                    b.Content.ToLower().Contains(term));
            }

            var orderedQuery = baseQuery
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .ThenByDescending(b => b.BlogPostId);

            // Featured post is the latest matching published article
            var featuredDto = await orderedQuery
                .Select(b => new BlogPostDto
                {
                    BlogPostId = b.BlogPostId,
                    Title = b.Title,
                    Slug = b.Slug,
                    ShortDescription = b.ShortDescription,
                    FeaturedImage = b.FeaturedImage,
                    BlogCategoryId = b.BlogCategoryId,
                    CategoryName = b.Category != null ? b.Category.CategoryName : "Insights",
                    CategorySlug = b.Category != null ? b.Category.Slug : null,
                    AuthorUserId = b.AuthorUserId,
                    AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                        ? b.AuthorName.Trim()
                        : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt ?? b.CreatedAt,
                    CreatedAt = b.CreatedAt
                })
                .FirstOrDefaultAsync();

            var totalCount = await orderedQuery.CountAsync();
            var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 1;
            if (page > totalPages) page = totalPages;

            var posts = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BlogPostDto
                {
                    BlogPostId = b.BlogPostId,
                    Title = b.Title,
                    Slug = b.Slug,
                    ShortDescription = b.ShortDescription,
                    FeaturedImage = b.FeaturedImage,
                    BlogCategoryId = b.BlogCategoryId,
                    CategoryName = b.Category != null ? b.Category.CategoryName : "Insights",
                    CategorySlug = b.Category != null ? b.Category.Slug : null,
                    AuthorUserId = b.AuthorUserId,
                    AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                        ? b.AuthorName.Trim()
                        : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt ?? b.CreatedAt,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return new PublicBlogListingDto
            {
                FeaturedPost = featuredDto,
                Posts = posts,
                Categories = categories,
                ActiveCategorySlug = categorySlug?.Trim(),
                ActiveCategoryName = activeCategoryName,
                SearchQuery = search?.Trim(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPosts = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<BlogPostDto?> GetPublicBlogBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;
            var normalized = slug.Trim().ToLowerInvariant();

            var b = await DataContext.BlogPosts
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x =>
                    x.IsPublished &&
                    x.Slug.ToLower() == normalized &&
                    (x.Category == null || x.Category.IsActive));

            if (b == null) return null;

            return new BlogPostDto
            {
                BlogPostId = b.BlogPostId,
                Title = b.Title,
                Slug = b.Slug,
                ShortDescription = b.ShortDescription,
                Content = SanitizeHtml(b.Content),
                FeaturedImage = b.FeaturedImage,
                BlogCategoryId = b.BlogCategoryId,
                CategoryName = b.Category != null ? b.Category.CategoryName : "Insights",
                CategorySlug = b.Category != null ? b.Category.Slug : null,
                AuthorUserId = b.AuthorUserId,
                AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                    ? b.AuthorName.Trim()
                    : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                IsPublished = b.IsPublished,
                PublishedAt = b.PublishedAt ?? b.CreatedAt,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            };
        }

        public async Task<List<BlogPostDto>> GetRecentPublishedBlogsAsync(int count = 4, int? excludeBlogPostId = null)
        {
            var query = DataContext.BlogPosts
                .AsNoTracking()
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Where(b => b.IsPublished && (b.Category == null || b.Category.IsActive));

            if (excludeBlogPostId.HasValue && excludeBlogPostId.Value > 0)
            {
                query = query.Where(b => b.BlogPostId != excludeBlogPostId.Value);
            }

            return await query
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .ThenByDescending(b => b.BlogPostId)
                .Take(count)
                .Select(b => new BlogPostDto
                {
                    BlogPostId = b.BlogPostId,
                    Title = b.Title,
                    Slug = b.Slug,
                    ShortDescription = b.ShortDescription,
                    FeaturedImage = b.FeaturedImage,
                    BlogCategoryId = b.BlogCategoryId,
                    CategoryName = b.Category != null ? b.Category.CategoryName : "Insights",
                    CategorySlug = b.Category != null ? b.Category.Slug : null,
                    AuthorUserId = b.AuthorUserId,
                    AuthorName = !string.IsNullOrWhiteSpace(b.AuthorName)
                        ? b.AuthorName.Trim()
                        : (b.Author != null && !string.IsNullOrWhiteSpace(b.Author.FullName) ? b.Author.FullName.Trim() : "Agam Estates Editorial Team"),
                    IsPublished = b.IsPublished,
                    PublishedAt = b.PublishedAt ?? b.CreatedAt,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<BlogCategoryDto>> GetPublicCategoriesAsync()
        {
            return await DataContext.BlogCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new BlogCategoryDto
                {
                    BlogCategoryId = c.BlogCategoryId,
                    CategoryName = c.CategoryName,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    SortOrder = c.SortOrder,
                    PublishedBlogsCount = c.BlogPosts.Count(p => p.IsPublished)
                })
                .ToListAsync();
        }
    }
}

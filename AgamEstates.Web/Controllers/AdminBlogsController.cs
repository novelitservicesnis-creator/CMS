using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Repository.ViewModel;
using AgamEstates.Web.Models;
using AgamEstates.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBlogsController : BaseController
    {
        private readonly AgamEntities _dbContext;
        private readonly IBlogRepository _blogRepository;
        private readonly IFileStorageService _fileStorageService;

        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".webp" };
        private static readonly string[] AllowedImageMimeTypes = { "image/png", "image/jpeg", "image/webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        public AdminBlogsController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<AdminBlogsController> logger,
            IBlogRepository blogRepository,
            IFileStorageService fileStorageService,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _dbContext = agamEntity;
            _blogRepository = blogRepository;
            _fileStorageService = fileStorageService;
        }

        private int? GetCurrentAdminUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int id))
            {
                return id;
            }
            return null;
        }

        private string GetCurrentAdminFullName()
        {
            var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrWhiteSpace(nameClaim))
            {
                return nameClaim.Trim();
            }

            var adminId = GetCurrentAdminUserId();
            if (adminId.HasValue)
            {
                var dbName = _dbContext.Users
                    .Where(u => u.UserId == adminId.Value)
                    .Select(u => u.FullName)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(dbName))
                {
                    return dbName.Trim();
                }
            }

            return "Agam Estates Editorial Team";
        }

        [HttpGet("/Admin/Blogs/Preview/{id:int}")]
        public async Task<IActionResult> Preview(int id)
        {
            var blog = await _blogRepository.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            blog.Content = _blogRepository.SanitizeHtml(blog.Content);
            blog.PublishedAt ??= blog.CreatedAt;

            var categories = await _blogRepository.GetPublicCategoriesAsync();
            var recentPosts = await _blogRepository.GetRecentPublishedBlogsAsync(count: 4, excludeBlogPostId: blog.BlogPostId);

            var vm = new PublicBlogDetailViewModel
            {
                Post = blog,
                Categories = categories,
                RecentPosts = recentPosts,
                IsPreviewMode = true
            };

            return View("~/Views/Blog/Detail.cshtml", vm);
        }

        [HttpGet("/Admin/Blogs")]
        public async Task<IActionResult> Index(string? search = null, int? categoryId = null, string? status = null)
        {
            ViewData["Title"] = "Blog Management";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = null }
            };

            var summary = await _blogRepository.GetSummaryCountsAsync();
            var blogs = await _blogRepository.GetAdminBlogsAsync(search, categoryId, status);
            var categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: false);

            var vm = new AdminBlogsPageViewModel
            {
                Summary = summary,
                Blogs = blogs,
                Categories = categories,
                Search = search,
                CategoryFilter = categoryId,
                StatusFilter = status
            };

            return View(vm);
        }

        [HttpGet("/Admin/Blogs/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var blog = await _blogRepository.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound(new { success = false, message = "Blog post not found." });
            }

            return Json(new
            {
                success = true,
                blog = new
                {
                    blog.BlogPostId,
                    blog.Title,
                    blog.Slug,
                    blog.ShortDescription,
                    blog.Content,
                    blog.FeaturedImage,
                    blog.CategoryName,
                    blog.AuthorName,
                    blog.IsPublished,
                    PublishedAtFormatted = blog.PublishedAt?.ToString("dd MMM yyyy, hh:mm tt") ?? "Not published",
                    CreatedAtFormatted = blog.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
                    UpdatedAtFormatted = blog.UpdatedAt?.ToString("dd MMM yyyy, hh:mm tt") ?? "—"
                }
            });
        }

        [HttpGet("/Admin/Blogs/Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create Blog";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = "/Admin/Blogs" },
                new BreadcrumbItem { Text = "Create Blog", Url = null }
            };

            var categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: true);
            var defaultAdminName = GetCurrentAdminFullName();
            var vm = new BlogFormViewModel
            {
                Categories = categories,
                PublishStatus = "Draft",
                AuthorUserId = GetCurrentAdminUserId(),
                AuthorName = defaultAdminName,
                DefaultAdminAuthorName = defaultAdminName,
                IsEditMode = false
            };

            return View("BlogForm", vm);
        }

        [HttpPost("/Admin/Blogs/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogFormViewModel model, string? submitAction = null)
        {
            ViewData["Title"] = "Create Blog";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = "/Admin/Blogs" },
                new BreadcrumbItem { Text = "Create Blog", Url = null }
            };

            var defaultAdminName = GetCurrentAdminFullName();
            model.Categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: true);
            model.AuthorUserId = GetCurrentAdminUserId();
            model.DefaultAdminAuthorName = defaultAdminName;
            model.AuthorName = !string.IsNullOrWhiteSpace(model.AuthorName)
                ? model.AuthorName.Trim()
                : "Agam Estates Editorial Team";
            model.IsEditMode = false;

            if (!string.IsNullOrWhiteSpace(submitAction))
            {
                if (submitAction.Equals("Publish", StringComparison.OrdinalIgnoreCase))
                {
                    model.PublishStatus = "Published";
                }
                else if (submitAction.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    model.PublishStatus = "Draft";
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please review the highlighted fields and correct any validation errors.";
                return View("BlogForm", model);
            }

            // Validate slug before saving image
            var rawSlug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug! : model.Title;
            var resolvedSlug = _blogRepository.GenerateSlug(rawSlug, 200);
            if (string.IsNullOrWhiteSpace(resolvedSlug))
            {
                ModelState.AddModelError(nameof(model.Slug), "Please enter a valid title or URL slug.");
                return View("BlogForm", model);
            }

            if (!await _blogRepository.IsBlogSlugUniqueAsync(resolvedSlug))
            {
                ModelState.AddModelError(nameof(model.Slug), $"The slug '{resolvedSlug}' is already in use. Please enter a unique slug.");
                TempData["ErrorMessage"] = $"The URL slug '{resolvedSlug}' already exists.";
                return View("BlogForm", model);
            }

            string? uploadedImagePath = null;
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                var (isValid, validationError, ext) = ValidateUploadedImage(model.FeaturedImageFile);
                if (!isValid)
                {
                    ModelState.AddModelError(nameof(model.FeaturedImageFile), validationError!);
                    TempData["ErrorMessage"] = validationError;
                    return View("BlogForm", model);
                }

                try
                {
                    uploadedImagePath = await _fileStorageService.SaveBlogImageAsync(model.FeaturedImageFile, ext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save blog featured image.");
                    TempData["ErrorMessage"] = "Unable to save featured image. Please check server storage permissions.";
                    return View("BlogForm", model);
                }
            }

            var isPublished = string.Equals(model.PublishStatus, "Published", StringComparison.OrdinalIgnoreCase);
            var adminId = GetCurrentAdminUserId();

            try
            {
                var result = await _blogRepository.CreateBlogAsync(new CreateUpdateBlogPostDto
                {
                    Title = model.Title,
                    Slug = resolvedSlug,
                    ShortDescription = model.ShortDescription,
                    Content = model.Content,
                    FeaturedImage = uploadedImagePath,
                    BlogCategoryId = model.BlogCategoryId,
                    AuthorUserId = adminId,
                    AuthorName = model.AuthorName,
                    IsPublished = isPublished,
                    PublishedAt = model.PublishedAt,
                    AdminUserId = adminId
                });

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                // DB save failed: delete newly uploaded image so no orphan file remains
                if (!string.IsNullOrWhiteSpace(uploadedImagePath))
                {
                    _fileStorageService.TryDeleteManagedBlogFile(uploadedImagePath);
                }

                TempData["ErrorMessage"] = result.Message;
                return View("BlogForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while creating blog post.");
                if (!string.IsNullOrWhiteSpace(uploadedImagePath))
                {
                    _fileStorageService.TryDeleteManagedBlogFile(uploadedImagePath);
                }

                TempData["ErrorMessage"] = "An unexpected error occurred while saving the blog article.";
                return View("BlogForm", model);
            }
        }

        [HttpGet("/Admin/Blogs/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _blogRepository.GetBlogByIdAsync(id);
            if (blog == null)
            {
                TempData["ErrorMessage"] = "The requested blog article could not be found.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Edit Blog";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = "/Admin/Blogs" },
                new BreadcrumbItem { Text = "Edit Blog", Url = null }
            };

            var categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: false);
            var defaultAdminName = GetCurrentAdminFullName();
            var vm = new BlogFormViewModel
            {
                BlogPostId = blog.BlogPostId,
                Title = blog.Title,
                Slug = blog.Slug,
                BlogCategoryId = blog.BlogCategoryId,
                ShortDescription = blog.ShortDescription ?? string.Empty,
                Content = blog.Content,
                ExistingFeaturedImage = blog.FeaturedImage,
                PublishStatus = blog.IsPublished ? "Published" : "Draft",
                PublishedAt = blog.PublishedAt,
                AuthorUserId = blog.AuthorUserId ?? GetCurrentAdminUserId(),
                AuthorName = !string.IsNullOrWhiteSpace(blog.AuthorName) ? blog.AuthorName.Trim() : defaultAdminName,
                DefaultAdminAuthorName = defaultAdminName,
                Categories = categories,
                IsEditMode = true
            };

            return View("BlogForm", vm);
        }

        [HttpPost("/Admin/Blogs/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogFormViewModel model, string? submitAction = null)
        {
            model.BlogPostId = id;
            model.IsEditMode = true;
            model.Categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: false);

            ViewData["Title"] = "Edit Blog";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = "/Admin/Blogs" },
                new BreadcrumbItem { Text = "Edit Blog", Url = null }
            };

            var existingBlog = await _blogRepository.GetBlogByIdAsync(id);
            if (existingBlog == null)
            {
                TempData["ErrorMessage"] = "Blog article not found.";
                return RedirectToAction(nameof(Index));
            }

            var defaultAdminName = GetCurrentAdminFullName();
            var previousImagePath = existingBlog.FeaturedImage;
            model.ExistingFeaturedImage = previousImagePath;
            model.AuthorUserId = existingBlog.AuthorUserId ?? GetCurrentAdminUserId();
            model.DefaultAdminAuthorName = defaultAdminName;
            model.AuthorName = !string.IsNullOrWhiteSpace(model.AuthorName)
                ? model.AuthorName.Trim()
                : "Agam Estates Editorial Team";

            if (!string.IsNullOrWhiteSpace(submitAction))
            {
                if (submitAction.Equals("Publish", StringComparison.OrdinalIgnoreCase))
                {
                    model.PublishStatus = "Published";
                }
                else if (submitAction.Equals("Draft", StringComparison.OrdinalIgnoreCase))
                {
                    model.PublishStatus = "Draft";
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the highlighted errors in the form.";
                return View("BlogForm", model);
            }

            var rawSlug = !string.IsNullOrWhiteSpace(model.Slug) ? model.Slug! : model.Title;
            var resolvedSlug = _blogRepository.GenerateSlug(rawSlug, 200);
            if (string.IsNullOrWhiteSpace(resolvedSlug))
            {
                ModelState.AddModelError(nameof(model.Slug), "Please enter a valid title or URL slug.");
                return View("BlogForm", model);
            }

            if (!await _blogRepository.IsBlogSlugUniqueAsync(resolvedSlug, id))
            {
                ModelState.AddModelError(nameof(model.Slug), $"The slug '{resolvedSlug}' is already in use by another article.");
                TempData["ErrorMessage"] = $"The URL slug '{resolvedSlug}' already exists.";
                return View("BlogForm", model);
            }

            string? newImagePath = null;
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                var (isValid, validationError, ext) = ValidateUploadedImage(model.FeaturedImageFile);
                if (!isValid)
                {
                    ModelState.AddModelError(nameof(model.FeaturedImageFile), validationError!);
                    TempData["ErrorMessage"] = validationError;
                    return View("BlogForm", model);
                }

                try
                {
                    newImagePath = await _fileStorageService.SaveBlogImageAsync(model.FeaturedImageFile, ext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save replacement blog image.");
                    TempData["ErrorMessage"] = "Unable to save the new featured image. Please verify storage permissions.";
                    return View("BlogForm", model);
                }
            }

            var isPublished = string.Equals(model.PublishStatus, "Published", StringComparison.OrdinalIgnoreCase);
            var adminId = GetCurrentAdminUserId();

            try
            {
                var result = await _blogRepository.UpdateBlogAsync(new CreateUpdateBlogPostDto
                {
                    BlogPostId = id,
                    Title = model.Title,
                    Slug = resolvedSlug,
                    ShortDescription = model.ShortDescription,
                    Content = model.Content,
                    FeaturedImage = newImagePath ?? previousImagePath,
                    BlogCategoryId = model.BlogCategoryId,
                    AuthorName = model.AuthorName,
                    IsPublished = isPublished,
                    PublishedAt = model.PublishedAt,
                    AdminUserId = adminId
                });

                if (result.Success)
                {
                    // DB update succeeded: safely delete old managed image if a new one was uploaded
                    if (!string.IsNullOrWhiteSpace(newImagePath) &&
                        !string.IsNullOrWhiteSpace(previousImagePath) &&
                        !string.Equals(previousImagePath, newImagePath, StringComparison.OrdinalIgnoreCase))
                    {
                        _fileStorageService.TryDeleteManagedBlogFile(previousImagePath);
                    }

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                // DB update failed: delete newly uploaded image and keep previous image intact
                if (!string.IsNullOrWhiteSpace(newImagePath))
                {
                    _fileStorageService.TryDeleteManagedBlogFile(newImagePath);
                }

                TempData["ErrorMessage"] = result.Message;
                return View("BlogForm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error while updating blog post {BlogPostId}.", id);
                if (!string.IsNullOrWhiteSpace(newImagePath))
                {
                    _fileStorageService.TryDeleteManagedBlogFile(newImagePath);
                }

                TempData["ErrorMessage"] = "An unexpected error occurred while updating the blog article.";
                return View("BlogForm", model);
            }
        }

        [HttpPost("/Admin/Blogs/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message, deletedFeaturedImage) = await _blogRepository.DeleteBlogAsync(id);
                if (success)
                {
                    if (!string.IsNullOrWhiteSpace(deletedFeaturedImage))
                    {
                        _fileStorageService.TryDeleteManagedBlogFile(deletedFeaturedImage);
                    }
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post {BlogPostId}.", id);
                TempData["ErrorMessage"] = "Unable to delete the blog article. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // BLOG CATEGORIES MANAGEMENT
        // ============================================================

        [HttpGet("/Admin/BlogCategories")]
        public async Task<IActionResult> Categories()
        {
            ViewData["Title"] = "Blog Categories";
            ViewData["Breadcrumbs"] = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Dashboard", Url = "/AdminPanel" },
                new BreadcrumbItem { Text = "Blog Management", Url = "/Admin/Blogs" },
                new BreadcrumbItem { Text = "Categories", Url = null }
            };

            var categories = await _blogRepository.GetAllCategoriesAsync(activeOnly: false);
            var vm = new AdminBlogCategoriesPageViewModel
            {
                Categories = categories
            };

            return View(vm);
        }

        [HttpPost("/Admin/BlogCategories/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateUpdateBlogCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid category name.";
                return RedirectToAction(nameof(Categories));
            }

            var result = await _blogRepository.CreateCategoryAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost("/Admin/BlogCategories/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CreateUpdateBlogCategoryDto model)
        {
            if (!ModelState.IsValid || model.BlogCategoryId <= 0)
            {
                TempData["ErrorMessage"] = "Please provide valid category details.";
                return RedirectToAction(nameof(Categories));
            }

            var result = await _blogRepository.UpdateCategoryAsync(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost("/Admin/BlogCategories/ToggleStatus/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            var result = await _blogRepository.ToggleCategoryStatusAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost("/Admin/BlogCategories/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _blogRepository.DeleteCategoryAsync(id);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Categories));
        }

        private (bool IsValid, string? ErrorMessage, string Extension) ValidateUploadedImage(IFormFile file)
        {
            if (file.Length > MaxImageSizeBytes)
            {
                return (false, "Featured image exceeds the maximum allowed size of 5 MB.", string.Empty);
            }

            var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext))
            {
                return (false, "Invalid image format. Allowed formats: PNG, JPG, JPEG, WEBP.", string.Empty);
            }

            var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();
            if (!AllowedImageMimeTypes.Contains(contentType))
            {
                return (false, "Invalid image MIME type. Allowed types: PNG, JPG, JPEG, WEBP.", string.Empty);
            }

            return (true, null, ext);
        }
    }
}

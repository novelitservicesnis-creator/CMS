using AgamEstates.Core;
using AgamEstates.Repository.Interface;
using AgamEstates.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace AgamEstates.Web.Controllers
{
    public class BlogController : BaseController
    {
        private readonly IBlogRepository _blogRepository;

        public BlogController(
            AgamEntities agamEntity,
            IConfiguration configuration,
            ILogger<BlogController> logger,
            IBlogRepository blogRepository,
            IMemoryCache? cache = null)
            : base(agamEntity, configuration, logger, cache)
        {
            _blogRepository = blogRepository;
        }

        [HttpGet("/Blog")]
        public async Task<IActionResult> Index(
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1)
        {
            ViewData["Title"] = "Insights & Journal — Agam Estates";
            ViewData["ActiveNav"] = "Blog";
            ViewData["MetaDescription"] = "Explore property market insights, project updates, home-buying guidance, and architectural perspectives from Agam Estates.";

            var model = await _blogRepository.GetPublicBlogListingAsync(category, search, page, pageSize: 9);
            return View(model);
        }

        [HttpGet("/Blog/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            ViewData["ActiveNav"] = "Blog";

            if (string.IsNullOrWhiteSpace(slug))
            {
                Response.StatusCode = 404;
                ViewData["Title"] = "Article Not Found — Agam Estates";
                return View("NotFound");
            }

            var post = await _blogRepository.GetPublicBlogBySlugAsync(slug);
            if (post == null || !post.IsPublished)
            {
                Response.StatusCode = 404;
                ViewData["Title"] = "Article Not Found — Agam Estates";
                return View("NotFound");
            }

            ViewData["Title"] = $"{post.Title} — Agam Estates";
            ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(post.ShortDescription)
                ? post.ShortDescription
                : $"{post.Title} — Insights & Journal by Agam Estates.";

            var categories = await _blogRepository.GetPublicCategoriesAsync();
            var recentPosts = await _blogRepository.GetRecentPublishedBlogsAsync(count: 4, excludeBlogPostId: post.BlogPostId);

            var vm = new PublicBlogDetailViewModel
            {
                Post = post,
                Categories = categories,
                RecentPosts = recentPosts
            };

            return View(vm);
        }
    }
}

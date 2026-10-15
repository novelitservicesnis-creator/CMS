using Microsoft.EntityFrameworkCore;
using VSMovingServices.Web.Data;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Services;

public class TestimonialService : ITestimonialService
{
    private readonly VSMovingDbContext _db;
    private readonly ILogger<TestimonialService> _logger;

    public TestimonialService(VSMovingDbContext db, ILogger<TestimonialService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<PublicTestimonialViewModel>> GetPublishedTestimonialsAsync()
    {
        return await _db.Testimonials
            .AsNoTracking()
            .Where(t => t.IsPublished)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => new PublicTestimonialViewModel
            {
                TestimonialId = t.TestimonialId,
                CustomerName = t.CustomerName,
                Location = t.Location,
                ReviewText = t.ReviewText,
                Rating = t.Rating < 1 ? 1 : (t.Rating > 5 ? 5 : t.Rating),
                DisplayOrder = t.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<TestimonialsListViewModel> GetAdminTestimonialsAsync(string? search, string? statusFilter)
    {
        var totalCount = await _db.Testimonials.CountAsync();
        var publishedCount = await _db.Testimonials.CountAsync(t => t.IsPublished);
        var unpublishedCount = totalCount - publishedCount;
        var maxOrder = await _db.Testimonials.Select(t => (int?)t.DisplayOrder).MaxAsync() ?? 0;

        var query = _db.Testimonials.AsNoTracking().AsQueryable();

        var normalizedStatus = (statusFilter ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedStatus == "published")
        {
            query = query.Where(t => t.IsPublished);
        }
        else if (normalizedStatus == "unpublished" || normalizedStatus == "hidden")
        {
            query = query.Where(t => !t.IsPublished);
        }

        var trimmedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedSearch))
        {
            var term = trimmedSearch.ToLower();
            query = query.Where(t =>
                t.CustomerName.ToLower().Contains(term) ||
                (t.Location != null && t.Location.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return new TestimonialsListViewModel
        {
            Testimonials = items,
            Search = trimmedSearch,
            StatusFilter = string.IsNullOrWhiteSpace(normalizedStatus) ? null : normalizedStatus,
            TotalCount = totalCount,
            PublishedCount = publishedCount,
            UnpublishedCount = unpublishedCount,
            NextDisplayOrder = maxOrder + 1
        };
    }

    public async Task<(bool Success, string Message, Testimonial? Data)> CreateAsync(SaveTestimonialInputModel input)
    {
        var customerName = input.CustomerName?.Trim() ?? string.Empty;
        var location = string.IsNullOrWhiteSpace(input.Location) ? null : input.Location.Trim();
        var reviewText = input.ReviewText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(customerName))
            return (false, "Customer Name is required.", null);
        if (customerName.Length > 120)
            return (false, "Customer Name cannot exceed 120 characters.", null);

        if (location != null && location.Length > 120)
            return (false, "Location cannot exceed 120 characters.", null);

        if (string.IsNullOrWhiteSpace(reviewText))
            return (false, "Review Text is required.", null);
        if (reviewText.Length > 1000)
            return (false, "Review Text cannot exceed 1000 characters.", null);

        if (input.Rating < 1 || input.Rating > 5)
            return (false, "Rating must be between 1 and 5 stars.", null);

        var displayOrder = input.DisplayOrder < 0 ? 1 : input.DisplayOrder;

        var entity = new Testimonial
        {
            CustomerName = customerName,
            Location = location,
            ReviewText = reviewText,
            Rating = input.Rating,
            DisplayOrder = displayOrder,
            IsPublished = input.IsPublished,
            CreatedAt = DateTime.Now
        };

        _db.Testimonials.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created testimonial #{Id} for {CustomerName}.", entity.TestimonialId, entity.CustomerName);
        return (true, $"Testimonial from '{entity.CustomerName}' added successfully.", entity);
    }

    public async Task<(bool Success, string Message, Testimonial? Data)> UpdateAsync(EditTestimonialInputModel input)
    {
        var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.TestimonialId == input.TestimonialId);
        if (entity == null)
            return (false, "Testimonial not found.", null);

        var customerName = input.CustomerName?.Trim() ?? string.Empty;
        var location = string.IsNullOrWhiteSpace(input.Location) ? null : input.Location.Trim();
        var reviewText = input.ReviewText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(customerName))
            return (false, "Customer Name is required.", null);
        if (customerName.Length > 120)
            return (false, "Customer Name cannot exceed 120 characters.", null);

        if (location != null && location.Length > 120)
            return (false, "Location cannot exceed 120 characters.", null);

        if (string.IsNullOrWhiteSpace(reviewText))
            return (false, "Review Text is required.", null);
        if (reviewText.Length > 1000)
            return (false, "Review Text cannot exceed 1000 characters.", null);

        if (input.Rating < 1 || input.Rating > 5)
            return (false, "Rating must be between 1 and 5 stars.", null);

        entity.CustomerName = customerName;
        entity.Location = location;
        entity.ReviewText = reviewText;
        entity.Rating = input.Rating;
        entity.DisplayOrder = input.DisplayOrder < 0 ? 1 : input.DisplayOrder;
        entity.IsPublished = input.IsPublished;
        entity.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated testimonial #{Id} for {CustomerName}.", entity.TestimonialId, entity.CustomerName);
        return (true, $"Testimonial from '{entity.CustomerName}' updated successfully.", entity);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int testimonialId)
    {
        var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.TestimonialId == testimonialId);
        if (entity == null)
            return (false, "Testimonial not found.");

        var customerName = entity.CustomerName;
        _db.Testimonials.Remove(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted testimonial #{Id} ({CustomerName}).", testimonialId, customerName);
        return (true, $"Testimonial from '{customerName}' has been deleted.");
    }

    public async Task<(bool Success, string Message, bool? IsPublished)> TogglePublishAsync(int testimonialId)
    {
        var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.TestimonialId == testimonialId);
        if (entity == null)
            return (false, "Testimonial not found.", null);

        entity.IsPublished = !entity.IsPublished;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        var stateText = entity.IsPublished ? "published to the website" : "unpublished and hidden from the website";
        return (true, $"Testimonial from '{entity.CustomerName}' is now {stateText}.", entity.IsPublished);
    }

    public async Task<(bool Success, string Message)> UpdateDisplayOrderAsync(int testimonialId, int displayOrder)
    {
        var entity = await _db.Testimonials.FirstOrDefaultAsync(t => t.TestimonialId == testimonialId);
        if (entity == null)
            return (false, "Testimonial not found.");

        if (displayOrder < 0)
            return (false, "Display Order must be 0 or greater.");

        entity.DisplayOrder = displayOrder;
        entity.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return (true, $"Display order updated to {displayOrder} for '{entity.CustomerName}'.");
    }
}

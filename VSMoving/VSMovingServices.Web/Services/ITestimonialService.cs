using VSMovingServices.Web.Models;
using VSMovingServices.Web.Models.Entities;

namespace VSMovingServices.Web.Services;

public interface ITestimonialService
{
    Task<List<PublicTestimonialViewModel>> GetPublishedTestimonialsAsync();
    Task<TestimonialsListViewModel> GetAdminTestimonialsAsync(string? search, string? statusFilter);
    Task<(bool Success, string Message, Testimonial? Data)> CreateAsync(SaveTestimonialInputModel input);
    Task<(bool Success, string Message, Testimonial? Data)> UpdateAsync(EditTestimonialInputModel input);
    Task<(bool Success, string Message)> DeleteAsync(int testimonialId);
    Task<(bool Success, string Message, bool? IsPublished)> TogglePublishAsync(int testimonialId);
    Task<(bool Success, string Message)> UpdateDisplayOrderAsync(int testimonialId, int displayOrder);
}

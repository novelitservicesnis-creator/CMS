using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSMovingServices.Web.Models;
using VSMovingServices.Web.Services;

namespace VSMovingServices.Web.Controllers;

[Authorize(Roles = "Admin")]
public class TestimonialsController : Controller
{
    private readonly ITestimonialService _testimonialService;
    private readonly ILogger<TestimonialsController> _logger;

    public TestimonialsController(
        ITestimonialService testimonialService,
        ILogger<TestimonialsController> logger)
    {
        _testimonialService = testimonialService;
        _logger = logger;
    }

    [HttpGet("/Admin/Testimonials")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? search = null, string? status = null)
    {
        var vm = await _testimonialService.GetAdminTestimonialsAsync(search, status);
        return View(vm);
    }

    [HttpPost("/Admin/Testimonials/Create")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] SaveTestimonialInputModel input)
    {
        try
        {
            var result = await _testimonialService.CreateAsync(input);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating testimonial.");
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to add testimonial. Please try again."
            });
        }
    }

    [HttpPost("/Admin/Testimonials/Edit")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] EditTestimonialInputModel input)
    {
        try
        {
            var result = await _testimonialService.UpdateAsync(input);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating testimonial #{Id}.", input.TestimonialId);
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to update testimonial. Please try again."
            });
        }
    }

    [HttpPost("/Admin/Testimonials/Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] int testimonialId)
    {
        try
        {
            var result = await _testimonialService.DeleteAsync(testimonialId);
            if (!result.Success)
            {
                return NotFound(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting testimonial #{Id}.", testimonialId);
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to delete testimonial. Please try again."
            });
        }
    }

    [HttpPost("/Admin/Testimonials/TogglePublish")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish([FromForm] int testimonialId)
    {
        try
        {
            var result = await _testimonialService.TogglePublishAsync(testimonialId);
            if (!result.Success)
            {
                return NotFound(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = new { testimonialId, isPublished = result.IsPublished }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error toggling publish status for testimonial #{Id}.", testimonialId);
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to update testimonial status. Please try again."
            });
        }
    }

    [HttpPost("/Admin/Testimonials/UpdateOrder")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder([FromForm] int testimonialId, [FromForm] int displayOrder)
    {
        try
        {
            var result = await _testimonialService.UpdateDisplayOrderAsync(testimonialId, displayOrder);
            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating display order for testimonial #{Id}.", testimonialId);
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to update display order. Please try again."
            });
        }
    }
}

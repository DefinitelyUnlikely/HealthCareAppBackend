using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
    }

    /// <summary>
    /// Creates feedback for a meeting. Only patients can create feedback.
    /// </summary>
    [Authorize(Roles = "Patient")]
    [HttpPost("")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto request)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        try
        {
            var result = await _feedbackService.CreateFeedbackAsync(request, userId);
            return CreatedAtAction(nameof(GetFeedback), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific feedback by Id.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeedback(Guid id)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var result = await _feedbackService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = "Feedback not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all feedback. Admins can see all, caregivers see their own.
    /// </summary>
    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<FeedbackResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllFeedback()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var result = await _feedbackService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Updates feedback. Only the patient who created it can update.
    /// </summary>
    [Authorize(Roles = "Patient")]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateFeedback(Guid id, [FromBody] UpdateFeedbackDto request)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        try
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, request, userId);
            if (result == null)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes feedback. Only the patient who created it or an admin can delete.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFeedback(Guid id)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        try
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id, userId);
            if (!result)
            {
                return NotFound(new { message = "Feedback not found" });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
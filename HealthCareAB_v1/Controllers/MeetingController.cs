using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService ?? throw new ArgumentNullException(nameof(meetingService));
    }

    /// <summary>
    /// Creates a temporary meeting
    /// </summary>
    [Authorize]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto request)
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        bool isAdmin = User.IsInRole("Admin");
        if (request.CaregiverId != userId && request.PatientId != userId && !isAdmin)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var result = await _meetingService.CreateAsync(request);
        if (!result.Success || result.Meeting is null)
        {
            return Conflict(new { message = result.Message });
        }
        return CreatedAtAction(nameof(GetMeeting), new { id = result.Meeting.Id }, result);
    }

    /// <summary>
    /// Gets a specific meeting by Id.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMeeting(Guid id)
    {

        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }
        bool isAdmin = User.IsInRole("Admin");

        var result = await _meetingService.GetMeetingAsync(id, userId, isAdmin);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific meeting by Id.
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmMeeting([FromBody] ConfirmMeetingDto request, Guid id)
    {
        if (request.MeetingId != id)
        {
            return BadRequest(new { message = "Meeting Id does not match" });
        }

        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var result = await _meetingService.ConfirmAsync(request, userId);
        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(result);
    }

    /// <summary>
    /// Cancels a specific meeting by Id. Requires notes to be set.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelMeeting([FromBody] CancelMeetingDto request, Guid id)
    {
        if (String.IsNullOrEmpty(request.Notes))
        {
            return BadRequest(new { message = "Must provide reason for cancellation" });
        }
        if (request.MeetingId != id)
        {
            return BadRequest(new { message = "Meeting Id does not match" });
        }
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var result = await _meetingService.CancelAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        return NoContent();
    }
}

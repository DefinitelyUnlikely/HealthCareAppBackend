using HealthCareAB_v1.DTOs.Availability;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpPost("set-available")]
    public async Task<IActionResult> SetAvailableAsync([FromBody] SetAvailabilityRequest request)
    {
        // Just copying MeetingController's auth checks to make sure we stay consistent with how we handle auth.
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var isAdmin = User.IsInRole("Admin");
        if (request.UserId != userId && !isAdmin)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        await _availabilityService.SetAvailableAsync(request.UserId, request.From, request.To);
        return Ok(); // TODO: Return something more useful
    }

    [HttpPost("set-unavailable")]
    public async Task<IActionResult> SetUnavailableAsync([FromBody] SetAvailabilityRequest request)
    {
        // Just copying MeetingController's auth checks to make sure we stay consistent with how we handle auth.
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        var isAdmin = User.IsInRole("Admin");
        if (request.UserId != userId && !isAdmin)
        {
            return Unauthorized(new { message = "Not authenticated" });
        }

        await _availabilityService.SetUnavailableAsync(request.UserId, request.From, request.To,
            request.ForceCancel ?? false);
        return Ok(); // TODO: Return something more useful
    }
}

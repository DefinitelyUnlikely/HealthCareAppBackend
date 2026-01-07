using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Controllers
{
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
        /// Gets a specific meeting by Id.
        /// </summary>
        [Authorize]
        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMeeting(Guid id)
        {
            bool isAdmin = false;
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out int userId))
            {
                return Unauthorized(new { message = "Not authenticated" });
            }
            var claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (claimRole is "Admin") isAdmin = true;


            var result = await _meetingService.GetMeetingAsync(id, userId, isAdmin);
            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }
            return Ok(result);
        }
    }
}

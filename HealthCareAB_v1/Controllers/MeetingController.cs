using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Constants;
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
        /// Creates a new meeting.
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto request)
        {
            var result = await _meetingService.CreateAsync(request);
            return CreatedAtAction(nameof(CreateMeeting), new { result.Id });
        }

        public async Task<IActionResult> ConfirmMeeting([FromBody] ConfirmMeetingDto request)
        {
            throw new NotImplementedException();
        }
    }
}

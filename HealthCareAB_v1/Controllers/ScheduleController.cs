using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScheduleController(IScheduleService scheduleService) : ControllerBase
{
    [HttpGet("{careGiverId:int}")]
    public async Task<IActionResult> GetSchedule([FromRoute] int careGiverId, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        try
        {
            return Ok(await scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, from, to));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
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
    [HttpGet]
    [Authorize] // possibly not needed, maybe anyone should be able to see the schedule?
    public async Task<IActionResult> GetSchedules()
    {
        return Ok();
    }
}
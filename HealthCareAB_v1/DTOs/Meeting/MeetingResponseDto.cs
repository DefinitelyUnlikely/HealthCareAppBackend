using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.DTOs;

public class MeetingResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Meeting? Meeting { get; set; }
}


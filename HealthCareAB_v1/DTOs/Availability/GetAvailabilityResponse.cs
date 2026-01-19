namespace HealthCareAB_v1.DTOs.Availability;

public class GetAvailabilityResponse
{
    public List<TimeSpan> AvailableTimes { get; set; } = [];
}
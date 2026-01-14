namespace HealthCareAB_v1.DTOs.Availability;

public class GetAvailabilityResponse
{
    // Assuming caregiver is available as default
    public List<TimeSpan> UnavailableTimes { get; set; } = [];
}
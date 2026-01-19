namespace HealthCareAB_v1.DTOs.Availability;

public class GetAvailabilityRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
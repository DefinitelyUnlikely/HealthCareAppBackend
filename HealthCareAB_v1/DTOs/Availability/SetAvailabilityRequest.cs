namespace HealthCareAB_v1.DTOs.Availability;

public class SetAvailabilityRequest
{
    public int UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool ForceCancel { get; set; }
}
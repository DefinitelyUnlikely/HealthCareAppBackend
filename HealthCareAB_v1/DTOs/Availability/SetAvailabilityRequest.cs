namespace HealthCareAB_v1.DTOs.Availability;

/// <summary>
/// DTO for setting availability.
/// Used for both setting availability and setting unavailability
/// </summary>
public class SetAvailabilityRequest
{
    public int UserId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool? ForceCancel { get; set; }
}
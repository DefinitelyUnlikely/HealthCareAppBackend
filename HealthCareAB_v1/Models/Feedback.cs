namespace HealthCareAB_v1.Models;

public class Feedback
{
    public required Guid Id { get; set; }
    public string? Review { get; set; }
    public required int Rating { get; set; }
    public required int PatientId { get; set; }
    public required int CaregiverId { get; set; }
    public Patient? Patient { get; set; }
    public Caregiver? Caregiver { get; set; }
    public Guid MeetingId { get; set; }
}

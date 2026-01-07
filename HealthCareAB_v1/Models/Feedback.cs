namespace HealthCareAB_v1.Models;

public class Feedback
{
    public required Guid Id { get; set; }
    public string? Review { get; set; }
    public required int Rating { get; set; }

    public required Patient Patient { get; set; }
    public required Caregiver Caregiver { get; set; }
}
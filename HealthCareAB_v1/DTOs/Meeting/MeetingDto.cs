using HealthCareAB_v1.Models;

public class MeetingDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool Canceled { get; set; } = false;
    public MeetingStatus Status { get; set; }
    public string? Notes { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string CaregiverName { get; set; } = string.Empty;
}

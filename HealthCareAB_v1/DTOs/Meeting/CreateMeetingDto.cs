namespace HealthCareAB_v1.DTOs;

public class CreateMeetingDto
{
    public Guid PatientId { get; set; }
    public Guid CaregiverId { get; set; }
    public DateTime StartTime { get; set; }
    public int Slots { get; set; } = 1;
}
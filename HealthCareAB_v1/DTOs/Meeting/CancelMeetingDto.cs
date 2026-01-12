namespace HealthCareAB_v1.DTOs;

public class CancelMeetingDto
{
    public Guid MeetingId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

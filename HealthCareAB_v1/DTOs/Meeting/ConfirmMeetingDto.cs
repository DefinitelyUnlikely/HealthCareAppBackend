using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs;

public class ConfirmMeetingDto
{
    [Required]
    public int PatientId { get; set; }
    [Required]
    public Guid MeetingId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

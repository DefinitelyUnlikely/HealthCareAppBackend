using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs;

public class ConfirmMeetingDto
{
    [Required]
    public int PatientId { get; set; }
    [Required]
    public int MeetingId { get; set; }
}
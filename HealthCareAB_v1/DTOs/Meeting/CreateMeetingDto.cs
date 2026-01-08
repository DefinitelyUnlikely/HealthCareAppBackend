using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs;

public class CreateMeetingDto
{
    [Required]
    public int PatientId { get; set; }
    [Required]
    public int CaregiverId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    public int Slots { get; set; } = 1;
}

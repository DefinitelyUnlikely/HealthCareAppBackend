using System.ComponentModel;
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
    [DefaultValue(1)]
    public int Slots { get; set; } = 1;
}

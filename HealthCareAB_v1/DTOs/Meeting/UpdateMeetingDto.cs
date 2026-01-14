using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs;

public class UpdateMeetingDto
{
    [Required]
    public Guid MeetingId { get; set; }
    [Required]
    public required string Notes { get; set; }
    public DateTime? StartTime { get; set; }
    [DefaultValue(1)]
    public int Slots { get; set; } = 1;
}

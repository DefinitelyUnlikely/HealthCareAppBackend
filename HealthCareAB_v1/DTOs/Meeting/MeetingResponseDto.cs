using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.DTOs;

public class MeetingResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public MeetingDto? Meeting { get; set; }

    public static MeetingResponseDto FromEntity(Meeting newMeeting)
    {
        return new MeetingResponseDto
        {
            Success = true,
            Meeting = new MeetingDto
            {
                Id = newMeeting.Id,
                StartTime = newMeeting.StartTime,
                EndTime = newMeeting.EndTime,
                Status = newMeeting.Status,
                Canceled = newMeeting.Canceled,
                Notes = newMeeting.Notes,
                PatientName = newMeeting.Patient?.Username ?? string.Empty,
                CaregiverName = newMeeting.Caregiver?.Username ?? string.Empty
            }
        };
    }
}

using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.DTOs;

public class MeetingsResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<MeetingDto> Meetings { get; set; } = [];

    public static MeetingsResponseDto FromEntity(List<Meeting> meetings)
    {
        var meetingDtos = new List<MeetingDto>();
        foreach (var meeting in meetings)
        {
            meetingDtos.Add(new MeetingDto
            {
                Id = meeting.Id,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Status = meeting.Status,
                Canceled = meeting.Canceled,
                Notes = meeting.Notes,
                PatientName = meeting.Patient?.Username ?? string.Empty,
                CaregiverName = meeting.Caregiver?.Username ?? string.Empty
            });
        }
        return new MeetingsResponseDto
        {
            Success = true,
            Meetings = meetingDtos,
        };
    }
}

using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _meetingRepository;
    public MeetingService(IMeetingRepository meetingRepository)
    {
        _meetingRepository = meetingRepository ?? throw new ArgumentNullException(nameof(meetingRepository));
    }

    /// <summary>
    /// When the user selects a timeslot
    /// Creates a new meeting in the database and sets the status to pending.
    /// The meeting 
    /// </summary>
    public async Task<MeetingResponseDto> CreateAsync(CreateMeetingDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var endTime = request.StartTime.AddMinutes(30 * request.Slots);

        var newMeeting = new Meeting
        {
            StartTime = request.StartTime,
            EndTime = endTime,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            PatientId = request.PatientId,
            CaregiverId = request.CaregiverId,
            Status = MeetingStatus.Pending,
        };

        if (await _meetingRepository.TimeUnavailableAsync(newMeeting))
        {
            return new MeetingResponseDto() { Success = false, Message = "Meeting time unavailable" };
        }
        await _meetingRepository.CreateAsync(newMeeting);
        await _meetingRepository.GetAsync(newMeeting.Id);
        var response = MeetingResponseDto.FromEntity(newMeeting);

        return response;
    }

    /// <summary>
    /// Gets a meeting by Id.
    /// </summary>
    public async Task<MeetingResponseDto> GetMeetingAsync(Guid id, int userId, bool isAdmin)
    {
        var meeting = await _meetingRepository.GetAsync(id);
        if (meeting is null) return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        var isParticipant = meeting.CaregiverId == userId || meeting.PatientId == userId;
        if (!isParticipant && !isAdmin)
        {
            // Return not found even if meeting exists.
            return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        }
        return MeetingResponseDto.FromEntity(meeting);
    }

    public Task<Meeting> ConfirmAsync(ConfirmMeetingDto request)
    {
        throw new NotImplementedException();
    }
}

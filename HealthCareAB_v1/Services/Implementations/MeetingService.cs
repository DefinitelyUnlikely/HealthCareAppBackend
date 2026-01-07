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

    public async Task<MeetingResponseDto> GetMeetingAsync(Guid id, int userId, bool isAdmin)
    {
        var meeting = await _meetingRepository.GetAsync(id);
        if (meeting is null) return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        var isParticipant = meeting.Caregiver.Id == userId || meeting.Patient.Id == userId;
        if (!isParticipant && !isAdmin)
        {
            // Return not found even if meeting exists.
            return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        }
        return new MeetingResponseDto { Success = true, Meeting = meeting };
    }

    public async Task<Meeting> ConfirmAsynx(ConfirmMeetingDto meeting)
    {
        throw new NotImplementedException();
    }
}

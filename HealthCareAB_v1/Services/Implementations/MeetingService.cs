using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Services;

public class MeetingService : IMeetingService
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly INotificationService _notificationService;

    public MeetingService(IMeetingRepository meetingRepository, INotificationService notificationService)
    {
        _meetingRepository = meetingRepository ?? throw new ArgumentNullException(nameof(meetingRepository));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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

    public async Task<MeetingResponseDto> ConfirmAsync(ConfirmMeetingDto request, int userId)
    {
        ArgumentNullException.ThrowIfNull(request);
        var meeting = await _meetingRepository.GetAsync(request.MeetingId);
        if (meeting is null)
        {
            return new MeetingResponseDto { Success = false, Message = "Booking expired" };
        }

        if (meeting.PatientId != userId)
            return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        if (meeting.Status != MeetingStatus.Pending)
            return new MeetingResponseDto { Success = false, Message = "Meeting already confirmed" };

        meeting.Notes = request.Notes;
        meeting.Status = MeetingStatus.Confirmed;
        meeting.ExpiresAt = null;
        await _meetingRepository.SaveChangesAsync();

        if (meeting.Patient is null)
        {
            // This should not be null, but if it is, just return the meeting.
            // Won't be any patient to notify anyway then.
            return MeetingResponseDto.FromEntity(meeting);
        }

        await _notificationService.SendNotificationAsync(new MeetingConfirmedEmailNotification()
        {
            RecipientUser = meeting.Patient,
            Meeting = meeting,
        });


        return MeetingResponseDto.FromEntity(meeting);
    }

    public async Task<MeetingResponseDto> CancelAsync(CancelMeetingDto request, int userId)
    {
        ArgumentNullException.ThrowIfNull(request);
        var meeting = await _meetingRepository.GetAsync(request.MeetingId);
        if (meeting is null)
        {
            return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        }
        var patientCancel = meeting.PatientId == userId;
        var caregiverCancel = meeting.CaregiverId == userId;
        if (!patientCancel && !caregiverCancel)
        {
            return new MeetingResponseDto { Success = false, Message = "Invalid user" };
        }
        if (meeting.Status != MeetingStatus.Confirmed)
        {
            return new MeetingResponseDto { Success = false, Message = "Can only cancel confirmed meetings" };
        }
        if (meeting.StartTime < DateTime.Now.AddHours(23) && patientCancel) // Extra lenience because of DST.
        {
            return new MeetingResponseDto { Success = false, Message = "Can only cancel meetings at least 24 hours ahead" };
        }

        meeting.Canceled = true;
        meeting.Notes = request.Notes;
        await _meetingRepository.SaveChangesAsync();

        return MeetingResponseDto.FromEntity(meeting);
    }
}

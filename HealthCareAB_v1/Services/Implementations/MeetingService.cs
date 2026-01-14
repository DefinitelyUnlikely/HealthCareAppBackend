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

    public async Task<List<MeetingDto>> GetMeetingsAsync(int userId, bool historic)
    {
        var meetings = await _meetingRepository.GetByUserIdAsync(userId, historic);
        List<MeetingDto> result = [];
        foreach (var meeting in meetings)
        {
            if (meeting.Canceled)
            {
                continue;
            }
            result.Add(new MeetingDto()
            {
                Id = meeting.Id,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Status = meeting.Status,
                Notes = meeting.Notes,
                PatientName = $"{meeting.Patient?.FirstName} {meeting.Patient?.LastName}",
                CaregiverName = $"{meeting.Caregiver?.FirstName} {meeting.Caregiver?.LastName}",
            });
        }
        return result;
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

        if (meeting.Patient?.Email is null)
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

        if (meeting.Patient?.Email is null)
        {
            return MeetingResponseDto.FromEntity(meeting);
        }

        await _notificationService.SendNotificationAsync(new MeetingCancelledEmailNotification()
        {
            RecipientUser = meeting.Patient,
            Meeting = meeting,
        });

        return MeetingResponseDto.FromEntity(meeting);
    }

    /// <summary>
    /// Updates a meeting.
    /// </summary>
    /// <remarks>
    /// If the meeting time should be updated a new meeting is created and the old one is canceled.
    /// Otherwise we just update existing meeting Notes.
    /// </remarks>
    public async Task<MeetingResponseDto> UpdateAsync(UpdateMeetingDto request, int userId)
    {
        ArgumentNullException.ThrowIfNull(request);
        var meeting = await _meetingRepository.GetAsync(request.MeetingId);
        if (meeting is null)
        {
            return new MeetingResponseDto { Success = false, Message = "Meeting not found" };
        }

        var patientUpdate = meeting.PatientId == userId;
        var caregiverUpdate = meeting.CaregiverId == userId;
        if (!patientUpdate && !caregiverUpdate)
        {
            return new MeetingResponseDto { Success = false, Message = "Invalid user" };
        }

        // Update notes and return updated Meeting
        if (request.StartTime is null)
        {
            meeting.Notes = request.Notes;
            await _meetingRepository.SaveChangesAsync();
            return MeetingResponseDto.FromEntity(meeting);
        }

        // Try to create new meeting and cancel old meeting
        if (meeting.StartTime < DateTime.Now.AddHours(23) && patientUpdate) // Extra lenience because of DST.
        {
            return new MeetingResponseDto { Success = false, Message = "Can only reschedule meetings at least 24 hours ahead" };
        }

        var newMeeting = new Meeting
        {
            StartTime = request.StartTime.Value,
            EndTime = request.StartTime.Value.AddMinutes(30 * request.Slots),
            PatientId = meeting.PatientId,
            CaregiverId = meeting.CaregiverId,
            Notes = request.Notes,
            Status = MeetingStatus.Confirmed,
        };

        if (await _meetingRepository.TimeUnavailableAsync(newMeeting))
        {
            return new MeetingResponseDto() { Success = false, Message = "Meeting time unavailable" };
        }

        // Cancel old meeting and create new one
        meeting.Canceled = true;
        meeting.Notes = request.Notes;
        await _meetingRepository.CreateAsync(newMeeting);
        await _meetingRepository.SaveChangesAsync();

        if (meeting.Patient?.Email is null)
        {
            return MeetingResponseDto.FromEntity(newMeeting);
        }

        await _notificationService.SendNotificationAsync(new MeetingUpdatedEmailNotification()
        {
            RecipientUser = meeting.Patient,
            NewMeeting = newMeeting,
            OldMeeting = meeting,
        });

        return MeetingResponseDto.FromEntity(newMeeting);
    }
}

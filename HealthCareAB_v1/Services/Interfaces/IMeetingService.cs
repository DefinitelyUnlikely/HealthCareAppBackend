using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IMeetingService
{
    public Task<MeetingResponseDto> CreateAsync(CreateMeetingDto request);
    public Task<MeetingResponseDto> ConfirmAsync(ConfirmMeetingDto request, int userId);
    public Task<MeetingResponseDto> GetMeetingAsync(Guid id, int userId, bool isAdmin);
    public Task<MeetingResponseDto> CancelAsync(CancelMeetingDto request, int userId);
    public Task<MeetingResponseDto> UpdateAsync(UpdateMeetingDto request, int userId);
}

using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IMeetingService
{
    public Task<Meeting> CreateAsync(CreateMeetingDto request);
    public Task<Meeting> ConfirmAsync(ConfirmMeetingDto request);
    public Task<MeetingResponseDto> GetMeetingAsync(Guid id, int userId, bool isAdmin);
}

using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IMeetingService
{
    public Task<MeetingResponseDto> GetMeetingAsync (Guid id, int userId);
}

using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IMeetingService
{
    public Task<Meeting> CreateAsync(CreateMeetingDto meeting);
    public Task<Meeting> ConfirmAsynx(ConfirmMeetingDto meeting);
}

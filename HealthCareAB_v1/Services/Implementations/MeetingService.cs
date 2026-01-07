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

    public async Task<Meeting> CreateAsync(CreateMeetingDto meeting)
    {
        throw new NotImplementedException();
    }
}

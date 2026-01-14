using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class AvailabilityService(IAvailabilityRepository availabilityRepository, IMeetingService meetingService)
    : IAvailabilityService
{
    // Assuming we are available by default, this method, somewhat ironically,
    // calls the delete operation from the repository. 
    public async Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        throw new NotImplementedException();
    }

    public async Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null,
        bool forceCancel = false)
    {
        if (forceCancel)
        {
            var meetings = await meetingService.GetMeetingsAsync(userId, false);
            foreach (var meeting in meetings)
            {
                await meetingService.CancelAsync(new CancelMeetingDto
                {
                    MeetingId = meeting.Id,
                    Notes = "Möte avbokat då vårdgivaren inte längre är tillgänglig."
                }, userId);
            }
        }

        // now to the tricky part...
        // We need to get all unavailabilites, including meetings.
        // And then we need to create a list of unavailabilities that fits between the meetings 
        // and prior unavailabilites. And in reality, we would probably like to make sure that 
        // we have as few unavailabilites as possible.
        var unavailabilities = await GetUnavailabilityAsync(userId, from, to, true);
    }

    public async Task<List<Unavailability>> GetUnavailabilityAsync(int userId, DateTime? from = null,
        DateTime? to = null, bool includeMeetings = false)
    {
        var unavailabilities = await availabilityRepository.GetUnavailabilityAsync(userId, from ?? DateTime.Now,
            to ?? DateTime.Now.AddMonths(3));

        if (!includeMeetings) return unavailabilities;

        var meetings = await meetingService.GetMeetingsAsync(userId, false);
        unavailabilities.AddRange(meetings.Select(m => new Unavailability
        {
            CaregiverId = userId,
            StartTime = m.StartTime,
            EndTime = m.EndTime
        }));

        return unavailabilities;
    }
}
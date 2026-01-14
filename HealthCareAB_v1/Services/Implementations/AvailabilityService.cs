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
        var meetings = await meetingService.GetMeetingsAsync(userId, false);
        if (forceCancel)
        {
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
        // if forceCancel is false, we need to get all meetings for the caregiver between from and to
        // just like before. (so we can put that outside the if statement). 
        // BUT we need to now create a list of unavailabilities that fits between the meetings.

        await availabilityRepository.SaveUnavailabilityAsync(new Unavailability
        {
            CaregiverId = userId,
            StartDate = from ?? DateTime.Now,
            EndDate = to ?? DateTime.Now.AddMonths(3)
        });
    }

    public async Task<List<Unavailability>> GetUnavailabilityAsync(int userId, DateTime? from = null,
        DateTime? to = null, bool includeMeetings = false)
    {
        return await availabilityRepository.GetUnavailabilityAsync(userId, from ?? DateTime.Now,
            to ?? DateTime.Now.AddMonths(3));
    }
}
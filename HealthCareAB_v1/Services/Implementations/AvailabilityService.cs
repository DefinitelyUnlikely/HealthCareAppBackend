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
        // Now this one is a bit more complex. At least when forceCancel is false.
        // If it is true, We "simply" get all meetings (for the caregiver) between from and to and delete them.
        // then we call the repo and set the range as unavailable.

        if (forceCancel)
        {
            // Get all meetings for the caregiver between from and to
            var meetings = await meetingService.GetMeetingsAsync(userId, false);
            // Delete all meetings for the caregiver between from and to
            foreach (var meeting in meetings)
            {
                await meetingService.CancelAsync(new CancelMeetingDto
                {
                    MeetingId = meeting.Id,
                    Notes = "Möte avbokad då vårdgivaren inte längre är tillgänglig."
                }, userId);
            }
        }

        throw new NotImplementedException();
    }

    public async Task<List<Availability>> GetUnavailabilityAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        return await availabilityRepository.GetUnavailabilityAsync(userId, from ?? DateTime.Now,
            to ?? DateTime.Now.AddMonths(3));
    }
}
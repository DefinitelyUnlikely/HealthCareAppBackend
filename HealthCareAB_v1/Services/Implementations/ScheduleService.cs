using HealthCareAB_v1.DTOs.Availability;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class ScheduleService(IAvailabilityService availabilityService, IMeetingRepository meetingRepository)
    : IScheduleService
{
    // This will be kept unimplemented for now. I do not have time to think about the 
    // requried logic for this method with the time we have left to work on the project.
    public async Task GetFreeTimeSlots(DateTime? from = null, DateTime? to = null)
    {
        // if (from > to)
        // {
        //     throw new ArgumentException("From date must be before to date");
        // }

        // var startTime = from ?? DateTime.Now;
        // var endTime = to ?? new DateTime(startTime.Year, startTime.Month,
        //     DateTime.DaysInMonth(startTime.Year, startTime.Month), 23, 59, 59);

        // if (endTime - startTime > TimeSpan.FromDays(90))
        // {
        //     throw new ArgumentException("To date must be within 90 days from from date");
        // }

        // var availableTimeSlots = await availabilityService.GetAvailabilityAsync(null, startTime, endTime);
        // var meetings = await meetingRepository.GetAllAsync(startTime, endTime);

        throw new NotImplementedException();
    }

    public async Task<List<AvailabilityDto>> GetFreeTimeSlotsForCareGiver(int careGiverId, DateTime? from = null,
        DateTime? to = null)
    {
        if (from > to)
        {
            throw new ArgumentException("From date must be before to date");
        }

        // Get times further in the future for individual caregiver
        var startTime = from ?? DateTime.Now;
        var endTime = to ?? new DateTime(startTime.Year, startTime.AddMonths(1).Month,
            DateTime.DaysInMonth(startTime.Year, startTime.AddMonths(1).Month), 23, 59, 59);

        if (endTime - startTime > TimeSpan.FromDays(90))
        {
            throw new ArgumentException("To date must be within 90 days from from date");
        }

        var availableTimeSlots = await availabilityService.GetAvailabilityAsync(careGiverId, startTime, endTime);
        var meetings = await meetingRepository.GetByUserIdAsync(careGiverId, false);

        var meetingsInTimeRange = meetings.Where(m => m.StartTime < endTime && m.EndTime > startTime)
            .OrderBy(m => m.StartTime).ToList();

        List<AvailabilityDto> freeTimeslots = [];

        foreach (var slot in availableTimeSlots)
        {
            var currentStart = slot.StartDate;
            var currentEnd = slot.EndDate;

            var overlappingMeetings = meetingsInTimeRange
                .Where(m => m.StartTime < currentEnd && m.EndTime > currentStart)
                .OrderBy(m => m.StartTime)
                .ToList();

            foreach (var meeting in overlappingMeetings)
            {
                if (meeting.StartTime > currentStart)
                {
                    freeTimeslots.Add(new AvailabilityDto
                    {
                        CareGiverIds = [slot.CaregiverId],
                        StartTime = currentStart,
                        EndTime = meeting.StartTime
                    });
                }

                if (meeting.EndTime > currentStart)
                {
                    currentStart = meeting.EndTime;
                }
            }

            if (currentStart < currentEnd)
            {
                freeTimeslots.Add(new AvailabilityDto
                {
                    CareGiverIds = [slot.CaregiverId],
                    StartTime = currentStart,
                    EndTime = currentEnd
                });
            }
        }

        return freeTimeslots;
    }
}

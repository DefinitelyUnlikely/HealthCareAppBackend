using HealthCareAB_v1.DTOs.Availability;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class ScheduleService(IAvailabilityService availabilityService, IMeetingRepository meetingRepository)
    : IScheduleService
{
    public async Task GetFreeTimeSlots(DateTime? from = null, DateTime? to = null)
    {
        if (from > to)
        {
            throw new ArgumentException("From date must be before to date");
        }

        var startTime = from ?? DateTime.Now;
        var endTime = to ?? new DateTime(startTime.Year, startTime.Month,
            DateTime.DaysInMonth(startTime.Year, startTime.Month), 23, 59, 59);

        if (endTime - startTime > TimeSpan.FromDays(90))
        {
            throw new ArgumentException("To date must be within 90 days from from date");
        }

        var availableTimeSlots = await availabilityService.GetAvailabilityAsync(null, startTime, endTime);
        var meetings = await meetingRepository.GetAllAsync(startTime, endTime);
    }

    public async Task GetFreeTimeSlotsForCareGiver(int careGiverId, DateTime? from = null, DateTime? to = null)
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

        var meetingsInTimeRange = meetings.Where(m => m.StartTime >= startTime && m.EndTime <= endTime);

        List<AvailabilityDto> freeTimeslots = [];

        foreach (var slot in availableTimeSlots)
        {
        }
    }
}

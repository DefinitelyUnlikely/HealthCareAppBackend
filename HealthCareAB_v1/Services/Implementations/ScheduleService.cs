using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class ScheduleService : IScheduleService
{
    public Task GetFreeTimeSlots(DateTime? from = null, DateTime? to = null)
    {
        if (from > to)
        {
            throw new ArgumentException("From date must be before to date");
        }

        var startTime = from ?? DateTime.Now;
        var endTime = to ?? new DateTime(startTime.Year, startTime.Month,
            DateTime.DaysInMonth(startTime.Year, startTime.Month), 23, 59, 59);

        throw new NotImplementedException();
    }

    public Task GetFreeTimeSlotsForCareGiver(int careGiverId, DateTime? from = null, DateTime? to = null)
    {
        if (from > to)
        {
            throw new ArgumentException("From date must be before to date");
        }

        // Get times further in the future for individual caregiver
        var startTime = from ?? DateTime.Now;
        var endTime = to ?? new DateTime(startTime.Year, startTime.AddMonths(1).Month,
            DateTime.DaysInMonth(startTime.Year, startTime.AddMonths(1).Month), 23, 59, 59);

        throw new NotImplementedException();
    }
}

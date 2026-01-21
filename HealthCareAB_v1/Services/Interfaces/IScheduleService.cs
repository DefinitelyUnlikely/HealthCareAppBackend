using HealthCareAB_v1.DTOs.Availability;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IScheduleService
{
    Task GetFreeTimeSlots(DateTime? from = null, DateTime? to = null);

    Task<List<AvailabilityDto>> GetFreeTimeSlotsForCareGiver(int careGiverId, DateTime? from = null,
        DateTime? to = null);
}
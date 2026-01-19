namespace HealthCareAB_v1.Services.Interfaces;

public interface IScheduleService
{
    Task GetFreeTimeSlots(DateTime? from = null, DateTime? to = null);
    Task GetFreeTimeSlotsForCareGiver(int careGiverId, DateTime? from = null, DateTime? to = null);
}
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations
{
    public class AvailabilityService(
        IAvailabilityRepository availabilityRepository,
        IMeetingRepository meetingRepository,
        IMeetingService meetingService) : IAvailabilityService
    {
        public async Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            var startTime = from ?? DateTime.Now;
            var endTime = to ?? DateTime.Now.AddMonths(3);

            if (startTime < startTime.Date.AddHours(8))
            {
                startTime = startTime.Date.AddHours(8);
            }

            if (endTime > endTime.Date.AddHours(16))
            {
                endTime = endTime.Date.AddHours(16);
            }

            for (var day = startTime.Date; day <= endTime.Date; day = day.AddDays(1))
            {
                var availability = new Availability
                {
                    CaregiverId = userId,
                    StartTime = day.AddHours(8),
                    EndTime = day.AddHours(16)
                };

                await availabilityRepository.SaveAvailabilityAsync(availability);
            }
        }

        public async Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null,
            bool forceCancel = false)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            var startTime = from ?? DateTime.Now;
            var endTime = to ?? DateTime.Now.AddMonths(3);

            // If forceCancel is not true and my new way of thinking of it works, 
            // all we need to do is remove the availability. 
            await availabilityRepository.DeleteAvailabilityAsync(userId, startTime, endTime);

            // then if forceCancel is true, we need to cancel all meetings in the range.
            if (forceCancel)
            {
                var meetings = await GetOverlappingMeetings(userId, startTime, endTime);
                foreach (var meeting in meetings)
                {
                    await meetingService.CancelAsync(new CancelMeetingDto
                    {
                        MeetingId = meeting.Id,
                        Notes = "Vårdgivaren är inte längre tillgänglig"
                    }, userId);
                }
            }
        }

        public async Task<List<Availability>> GetAvailabilityAsync(int userId, DateTime? from = null,
            DateTime? to = null)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            var startTime = from ?? DateTime.Now;
            var endTime = to ?? DateTime.Now.AddMonths(3);

            var availability = await availabilityRepository.GetAvailabilityAsync(userId, startTime, endTime);

            return availability;
        }

        public async Task<List<Meeting>> GetOverlappingMeetings(int userId, DateTime from, DateTime to)
        {
            var meetings = await meetingRepository.GetByUserIdAsync(userId, false);
            var relevantMeetings = meetings
                .Where(m => !m.Canceled && m.StartTime < to && m.EndTime > from)
                .OrderBy(m => m.StartTime)
                .ToList();

            return relevantMeetings;
        }
    }
}

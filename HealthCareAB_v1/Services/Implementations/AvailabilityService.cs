using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations
{
    public class AvailabilityService(
        IAvailabilityRepository availabilityRepository,
        IMeetingRepository meetingRepository) : IAvailabilityService
    {
        public async Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            var availability = new Availability
            {
                CaregiverId = userId,
                StartTime = from ?? DateTime.Now,
                EndTime = to ?? DateTime.Now.AddMonths(3)
            };

            await availabilityRepository.SaveAvailabilityAsync(availability);
        }

        public async Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null,
            bool forceCancel = false)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            throw new NotImplementedException();
        }

        public async Task<List<Availability>> GetAvailabilityAsync(int userId, DateTime? from = null,
            DateTime? to = null,
            bool includeMeetings = false)
        {
            if (from > to)
            {
                throw new ArgumentException("Start time of range cannot be larger than end time of range");
            }

            // Get all availabilities, that are relevant to the timerange (i.e. overlap with the timerange)
            var availability = await availabilityRepository.GetAvailabilityAsync(userId, from, to);

            if (!includeMeetings)
            {
                return availability;
            }

            // Get all meetings, that are relevant to the timerange (i.e. overlap with the timerange)
            // Decent chance I'm thinking about this in reverse...
            var meetings = await meetingRepository.GetByUserIdAsync(userId, false);
            var relevantMeetings = meetings
                .Where(m => !m.Canceled && m.StartTime < (to ?? DateTime.Now.AddMonths(3)) &&
                            m.EndTime > (from ?? DateTime.Now))
                .OrderBy(m => m.StartTime)
                .ToList();

            if (relevantMeetings.Count == 0)
            {
                return availability;
            }

            var actualAvailabilities = new List<Availability>();

            // Can't think of a way to do this without a nested loop... 
            // I could possibly create some sort of recursive function? 
            foreach (var a in availability)
            {
                var slotStart = a.StartTime;
                var slotEnd = a.EndTime;

                var overlappingMeetings = relevantMeetings
                    .Where(m => m.StartTime < slotEnd && m.EndTime > slotStart)
                    .OrderBy(m => m.StartTime)
                    .ToList();

                foreach (var meeting in overlappingMeetings)
                {
                    if (slotStart < meeting.StartTime)
                    {
                        actualAvailabilities.Add(new Availability
                        {
                            CaregiverId = userId,
                            StartTime = slotStart,
                            EndTime = meeting.StartTime
                        });
                    }

                    if (meeting.EndTime > slotStart)
                    {
                        slotStart = meeting.EndTime;
                    }
                }

                if (slotStart < slotEnd)
                {
                    actualAvailabilities.Add(new Availability
                    {
                        CaregiverId = userId,
                        StartTime = slotStart,
                        EndTime = slotEnd
                    });
                }
            }

            return actualAvailabilities;
        }
    }
}

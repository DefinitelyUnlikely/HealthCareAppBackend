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
            // I'm throwing an exception here
            // but we could invert the from and to values
            // as a sort of assumption that the user simply put in the dates in the wrong order.
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

            if (!forceCancel)
            {
                await availabilityRepository.DeleteAvailabilityAsync(userId, from, to);
                return;
            }

            // if we force cancel, we need to delete the availability and all meetings in that range
            await availabilityRepository.DeleteAvailabilityAsync(userId, from, to);

            // still uncertain I am thinking about this correctly... But I'll copy the 
            // logic to get relevant meetings from GetAvailabilityAsync.
            // Althought... When getting Availability, we want to get the overlapping meetings cause those
            // do indeed make you unavailable for the time that overlaps with your availability...
            // But when you're forcing a cancel, should we only cancel meetings that's squarly inside the timerange?
            // Or should we cancel all meetings that overlap with the timerange? I'll go with that for now.
            var relevantMeetings =
                await GetOverlappingMeetings(userId, from ?? DateTime.Now, to ?? DateTime.Now.AddMonths(3));

            foreach (var meeting in relevantMeetings)
            {
                await meetingService.CancelAsync(new CancelMeetingDto
                {
                    MeetingId = meeting.Id,
                    Notes = "Vårdgivaren är inte längre tillgänglig"
                }, userId);
            }
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
            var relevantMeetings =
                await GetOverlappingMeetings(userId, from ?? DateTime.Now, to ?? DateTime.Now.AddMonths(3));

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

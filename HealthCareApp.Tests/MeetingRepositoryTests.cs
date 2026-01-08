using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models;
using Microsoft.EntityFrameworkCore;
using HealthCareAB_v1.Repositories;

namespace HealthCareApp.Tests;

public class MeetingRepositoryTests
{

    [Fact]
    public async Task MeetingTimeDoesNotOverlap_ReturnsFalse()
    {
        // Arrange
        var context = TestContext();
        var validMeetingTime = new Meeting
        {
            CaregiverId = 1,
            StartTime = new DateTime(2024, 1, 1, 9, 30, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        var repo = new MeetingRepository(context);

        var exists = await repo.TimeUnavailableAsync(validMeetingTime);

        Assert.False(exists);
    }


    [Fact]
    public async Task MeetingTimeOverlaps_ReturnsTrue()
    {
        // Arrange
        var context = TestContext();
        var invalidMeetingTime = new Meeting
        {
            CaregiverId = 1,
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 30, 0)
        };

        var repo = new MeetingRepository(context);

        var exists = await repo.TimeUnavailableAsync(invalidMeetingTime);

        Assert.True(exists);
    }

    private AppDbContext TestContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

        var context = new AppDbContext(options);
        var existingMeetings = new List<Meeting>
        {
            new Meeting
            {
                CaregiverId = 1,
                StartTime = new DateTime(2024, 1, 1, 9, 0, 0),
                EndTime   = new DateTime(2024, 1, 1, 9, 30, 0)
            },
            new Meeting
            {
                CaregiverId = 1,
                StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
                EndTime   = new DateTime(2024, 1, 1, 11, 0, 0)
            },
            new Meeting
            {
                CaregiverId = 1,
                StartTime = new DateTime(2024, 1, 1, 11, 0, 0),
                EndTime   = new DateTime(2024, 1, 1, 12, 0, 0)
            }
        };

        context.Meetings.AddRange(existingMeetings);
        context.SaveChanges();

        return context;
    }
}

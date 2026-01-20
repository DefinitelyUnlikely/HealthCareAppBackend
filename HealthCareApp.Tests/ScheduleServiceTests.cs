using HealthCareAB_v1.DTOs.Availability;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Services.Interfaces;
using Moq;
using Xunit;

namespace HealthCareApp.Tests;

public class ScheduleServiceTests
{
    private readonly Mock<IAvailabilityService> _mockAvailabilityService;
    private readonly Mock<IMeetingRepository> _mockMeetingRepository;
    private readonly ScheduleService _scheduleService;

    public ScheduleServiceTests()
    {
        _mockAvailabilityService = new Mock<IAvailabilityService>();
        _mockMeetingRepository = new Mock<IMeetingRepository>();
        _scheduleService = new ScheduleService(_mockAvailabilityService.Object, _mockMeetingRepository.Object);
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_NoMeetings_ReturnsOriginalSlots()
    {
        // Arrange
        var careGiverId = 1;
        var start = DateTime.Now;
        var end = start.AddHours(8);
        var availabilitySlots = new List<Availability>
        {
            new() { CaregiverId = careGiverId, StartTime = start, EndTime = end }
        };

        _mockAvailabilityService
            .Setup(s => s.GetAvailabilityAsync(careGiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(availabilitySlots);
        _mockMeetingRepository.Setup(r => r.GetByUserIdAsync(careGiverId, false))
            .ReturnsAsync(new List<Meeting>());

        // Act
        var result = await _scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, start, end);

        // Assert
        Assert.Single(result);
        Assert.Equal(start, result[0].StartTime);
        Assert.Equal(end, result[0].EndTime);
        Assert.Contains(careGiverId, result[0].CareGiverIds);
    }
}

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

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_MeetingSplitsSlot_ReturnsTwoSlots()
    {
        // Arrange
        var careGiverId = 1;
        var start = DateTime.Now.Date.AddHours(8); // 08:00
        var end = start.AddHours(8); // 16:00
        var availabilitySlots = new List<Availability>
        {
            new() { CaregiverId = careGiverId, StartTime = start, EndTime = end }
        };

        var meetingStart = start.AddHours(3); // 11:00
        var meetingEnd = start.AddHours(4); // 12:00
        var meetings = new List<Meeting>
        {
            new() { CaregiverId = careGiverId, StartTime = meetingStart, EndTime = meetingEnd }
        };

        _mockAvailabilityService
            .Setup(s => s.GetAvailabilityAsync(careGiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(availabilitySlots);
        _mockMeetingRepository.Setup(r => r.GetByUserIdAsync(careGiverId, false))
            .ReturnsAsync(meetings);

        // Act
        var result = await _scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, start, end);

        // Assert
        Assert.Equal(2, result.Count);
        // Slot 1: 08:00 - 11:00
        Assert.Equal(start, result[0].StartTime);
        Assert.Equal(meetingStart, result[0].EndTime);
        // Slot 2: 12:00 - 16:00
        Assert.Equal(meetingEnd, result[1].StartTime);
        Assert.Equal(end, result[1].EndTime);
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_MeetingAtStart_ReturnsOneSlotAfter()
    {
        // Arrange
        var careGiverId = 1;
        var start = DateTime.Now.Date.AddHours(8);
        var end = start.AddHours(8);
        var availabilitySlots = new List<Availability>
        {
            new() { CaregiverId = careGiverId, StartTime = start, EndTime = end }
        };

        var meetingStart = start;
        var meetingEnd = start.AddHours(2);
        var meetings = new List<Meeting>
        {
            new() { CaregiverId = careGiverId, StartTime = meetingStart, EndTime = meetingEnd }
        };

        _mockAvailabilityService
            .Setup(s => s.GetAvailabilityAsync(careGiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(availabilitySlots);
        _mockMeetingRepository.Setup(r => r.GetByUserIdAsync(careGiverId, false))
            .ReturnsAsync(meetings);

        // Act
        var result = await _scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, start, end);

        // Assert
        Assert.Single(result);
        Assert.Equal(meetingEnd, result[0].StartTime);
        Assert.Equal(end, result[0].EndTime);
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_MeetingAtEnd_ReturnsOneSlotBefore()
    {
        // Arrange
        var careGiverId = 1;
        var start = DateTime.Now.Date.AddHours(8);
        var end = start.AddHours(8);
        var availabilitySlots = new List<Availability>
        {
            new() { CaregiverId = careGiverId, StartTime = start, EndTime = end }
        };

        var meetingStart = end.AddHours(-2);
        var meetingEnd = end;
        var meetings = new List<Meeting>
        {
            new() { CaregiverId = careGiverId, StartTime = meetingStart, EndTime = meetingEnd }
        };

        _mockAvailabilityService
            .Setup(s => s.GetAvailabilityAsync(careGiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(availabilitySlots);
        _mockMeetingRepository.Setup(r => r.GetByUserIdAsync(careGiverId, false))
            .ReturnsAsync(meetings);

        // Act
        var result = await _scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, start, end);

        // Assert
        Assert.Single(result);
        Assert.Equal(start, result[0].StartTime);
        Assert.Equal(meetingStart, result[0].EndTime);
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_InvalidDateRange_ThrowsException()
    {
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _scheduleService.GetFreeTimeSlotsForCareGiver(1, from, to));
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_MaxDateRangeExceeded_ThrowsException()
    {
        var from = DateTime.Now;
        var to = from.AddDays(91);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _scheduleService.GetFreeTimeSlotsForCareGiver(1, from, to));
    }

    [Fact]
    public async Task GetFreeTimeSlotsForCareGiver_MultipleMeetingsInOneSlot_ReturnsThreeSlots()
    {
        // Arrange
        var careGiverId = 1;
        var start = DateTime.Now.Date.AddHours(8); // 08:00
        var end = start.AddHours(8); // 16:00
        var availabilitySlots = new List<Availability>
        {
            new() { CaregiverId = careGiverId, StartTime = start, EndTime = end }
        };

        var meeting1Start = start.AddHours(2); // 10:00
        var meeting1End = start.AddHours(3); // 11:00
        var meeting2Start = start.AddHours(5); // 13:00
        var meeting2End = start.AddHours(6); // 14:00

        var meetings = new List<Meeting>
        {
            new() { CaregiverId = careGiverId, StartTime = meeting1Start, EndTime = meeting1End },
            new() { CaregiverId = careGiverId, StartTime = meeting2Start, EndTime = meeting2End }
        };

        _mockAvailabilityService
            .Setup(s => s.GetAvailabilityAsync(careGiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(availabilitySlots);
        _mockMeetingRepository.Setup(r => r.GetByUserIdAsync(careGiverId, false))
            .ReturnsAsync(meetings);

        // Act
        var result = await _scheduleService.GetFreeTimeSlotsForCareGiver(careGiverId, start, end);

        // Assert
        Assert.Equal(3, result.Count);
        // Slot 1: 08:00 - 10:00
        Assert.Equal(start, result[0].StartTime);
        Assert.Equal(meeting1Start, result[0].EndTime);
        // Slot 2: 11:00 - 13:00
        Assert.Equal(meeting1End, result[1].StartTime);
        Assert.Equal(meeting2Start, result[1].EndTime);
        // Slot 3: 14:00 - 16:00
        Assert.Equal(meeting2End, result[2].StartTime);
        Assert.Equal(end, result[2].EndTime);
    }
}

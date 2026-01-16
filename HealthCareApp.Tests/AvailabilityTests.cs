using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Services.Interfaces;
using Moq;
using System.Linq.Expressions;

namespace HealthCareApp.Tests;

public class AvailabilityTests
{
    private readonly Mock<IAvailabilityRepository> _availabilityRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<IMeetingService> _meetingServiceMock;
    private readonly AvailabilityService _availabilityService;

    public AvailabilityTests()
    {
        _availabilityRepositoryMock = new Mock<IAvailabilityRepository>();
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _meetingServiceMock = new Mock<IMeetingService>();
        _availabilityService = new AvailabilityService(
            _availabilityRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _meetingServiceMock.Object);
    }

    [Fact]
    public async Task SetAvailableAsync_WithValidDates_ShouldSaveAvailability()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now;
        var to = DateTime.Now.AddMonths(3);

        // Act
        await _availabilityService.SetAvailableAsync(userId, from, to);

        // Assert
        _availabilityRepositoryMock.Verify(r => r.SaveAvailabilityAsync(It.Is<Availability>(a =>
            a.CaregiverId == userId &&
            a.StartTime == from &&
            a.EndTime == to)), Times.Once);
    }

    [Fact]
    public async Task SetAvailableAsync_WithInvalidRange_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now; // End is before start

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.SetAvailableAsync(userId, from, to));
    }

    [Fact]
    public async Task SetUnavailableAsync_WithInvalidRange_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.SetUnavailableAsync(userId, from, to));
    }

    [Fact]
    public async Task SetUnavailableAsync_NoForceCancel_ShouldDeleteAvailabilityOnly()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);

        // Act
        await _availabilityService.SetUnavailableAsync(userId, from, to, forceCancel: false);

        // Assert
        _availabilityRepositoryMock.Verify(r => r.DeleteAvailabilityAsync(userId, from, to), Times.Once);
        _meetingRepositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        _meetingServiceMock.Verify(s => s.CancelAsync(It.IsAny<CancelMeetingDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SetUnavailableAsync_ForceCancel_ShouldDeleteAvailabilityAndCancelMeetings()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(2),
            EndTime = from.AddHours(3),
            Canceled = false
        };

        _meetingRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting });

        // Act
        await _availabilityService.SetUnavailableAsync(userId, from, to, forceCancel: true);

        // Assert
        _availabilityRepositoryMock.Verify(r => r.DeleteAvailabilityAsync(userId, from, to), Times.Once);
        _meetingServiceMock.Verify(s => s.CancelAsync(It.Is<CancelMeetingDto>(d =>
            d.MeetingId == meeting.Id &&
            d.Notes == "Vårdgivaren är inte längre tillgänglig"), userId), Times.Once);
    }

    [Fact]
    public async Task GetAvailabilityAsync_WithInvalidRange_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.GetAvailabilityAsync(userId, from, to));
    }

    [Fact]
    public async Task GetAvailabilityAsync_NoMeetingsIncluded_ShouldReturnRawAvailability()
    {
        // Arrange
        var userId = 1;
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);
        var availabilities = new List<Availability>
        {
            new Availability { CaregiverId = userId, StartTime = from, EndTime = to }
        };

        _availabilityRepositoryMock.Setup(r => r.GetAvailabilityAsync(userId, from, to))
            .ReturnsAsync(availabilities);

        // Act
        var result = await _availabilityService.GetAvailabilityAsync(userId, from, to, includeMeetings: false);

        // Assert
        Assert.Single(result);
        Assert.Equal(from, result[0].StartTime);
        Assert.Equal(to, result[0].EndTime);
        _meetingRepositoryMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailabilityAsync_IncludeMeetings_ShouldSplitAvailabilityAroundMeetings()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.Today.AddHours(12); // Noon
        var rangeStart = date;
        var rangeEnd = date.AddHours(4); // 12:00 - 16:00

        var availabilities = new List<Availability>
        {
            new Availability { CaregiverId = userId, StartTime = rangeStart, EndTime = rangeEnd }
        };

        // Meeting is 13:00 - 14:00, creating a hole in availability
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = rangeStart.AddHours(1),
            EndTime = rangeStart.AddHours(2),
            Canceled = false
        };

        _availabilityRepositoryMock.Setup(r => r.GetAvailabilityAsync(userId, rangeStart, rangeEnd))
            .ReturnsAsync(availabilities);

        _meetingRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting });

        // Act
        var result =
            await _availabilityService.GetAvailabilityAsync(userId, rangeStart, rangeEnd, includeMeetings: true);

        // Assert
        // We expect two availability slots: 12:00-13:00 and 14:00-16:00
        Assert.Equal(2, result.Count);

        Assert.Equal(rangeStart, result[0].StartTime);
        Assert.Equal(meeting.StartTime, result[0].EndTime); // 12:00 - 13:00

        Assert.Equal(meeting.EndTime, result[1].StartTime);
        Assert.Equal(rangeEnd, result[1].EndTime); // 14:00 - 16:00
    }

    [Fact]
    public async Task GetAvailabilityAsync_IncludeMeetings_MeetingOverlapsFully_ShouldConsumeSlot()
    {
        // Arrange
        var userId = 1;
        var date = DateTime.Today.AddHours(12);
        var rangeStart = date;
        var rangeEnd = date.AddHours(1);

        var availabilities = new List<Availability>
        {
            new Availability { CaregiverId = userId, StartTime = rangeStart, EndTime = rangeEnd }
        };

        // Meeting covers the entire slot
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = rangeStart,
            EndTime = rangeEnd,
            Canceled = false
        };

        _availabilityRepositoryMock.Setup(r => r.GetAvailabilityAsync(userId, rangeStart, rangeEnd))
            .ReturnsAsync(availabilities);

        _meetingRepositoryMock.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting });

        // Act
        var result =
            await _availabilityService.GetAvailabilityAsync(userId, rangeStart, rangeEnd, includeMeetings: true);

        // Assert
        Assert.Empty(result);
    }
}

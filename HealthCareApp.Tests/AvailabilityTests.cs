using Moq;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.DTOs;

namespace HealthCareApp.Tests;

public class AvailabilityTests
{
    private readonly Mock<IAvailabilityRepository> _mockAvailabilityRepo;
    private readonly Mock<IMeetingRepository> _mockMeetingRepo;
    private readonly Mock<IMeetingService> _mockMeetingService;
    private readonly AvailabilityService _availabilityService;

    public AvailabilityTests()
    {
        _mockAvailabilityRepo = new Mock<IAvailabilityRepository>();
        _mockMeetingRepo = new Mock<IMeetingRepository>();
        _mockMeetingService = new Mock<IMeetingService>();
        _availabilityService = new AvailabilityService(
            _mockAvailabilityRepo.Object,
            _mockMeetingRepo.Object,
            _mockMeetingService.Object
        );
    }

    [Fact]
    public async Task SetAvailableAsync_InvalidRange_ThrowsArgumentException()
    {
        // Arrange
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.SetAvailableAsync(1, from, to));
    }

    [Fact]
    public async Task SetAvailableAsync_ValidRange_SavesAvailabilityForEachDay()
    {
        // Arrange
        var from = new DateTime(2024, 1, 1, 8, 0, 0);
        var to = new DateTime(2024, 1, 3, 16, 0, 0);

        // Act
        await _availabilityService.SetAvailableAsync(1, from, to);

        // Assert
        _mockAvailabilityRepo.Verify(r => r.SaveAvailabilityAsync(It.IsAny<Availability>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SetAvailableAsync_ClampsTimesToWorkingHours()
    {
        // Arrange
        // Start before 8am
        var from = new DateTime(2024, 1, 1, 6, 0, 0);
        // End after 4pm
        var to = new DateTime(2024, 1, 1, 18, 0, 0);

        // Act
        await _availabilityService.SetAvailableAsync(1, from, to);

        // Assert
        _mockAvailabilityRepo.Verify(r => r.SaveAvailabilityAsync(It.Is<Availability>(a =>
            a.StartDate.Hour == 8 && a.EndDate.Hour == 16)), Times.Once);
    }

    [Fact]
    public async Task SetUnavailableAsync_InvalidRange_ThrowsArgumentException()
    {
        // Arrange
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.SetUnavailableAsync(1, from, to));
    }

    [Fact]
    public async Task SetUnavailableAsync_ValidRange_DeletesAvailability()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);

        // Act
        await _availabilityService.SetUnavailableAsync(1, from, to);

        // Assert
        _mockAvailabilityRepo.Verify(r => r.DeleteAvailabilityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task SetUnavailableAsync_ForceCancel_CancelsOverlappingMeetings()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);
        var userId = 1;

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(1),
            EndTime = from.AddHours(2),
            Canceled = false
        };

        _mockMeetingRepo.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting });

        // Act
        await _availabilityService.SetUnavailableAsync(userId, from, to, forceCancel: true);

        // Assert
        _mockMeetingService.Verify(s => s.CancelAsync(It.Is<CancelMeetingDto>(d => d.MeetingId == meeting.Id), userId),
            Times.Once);
    }

    [Fact]
    public async Task SetUnavailableAsync_NoForceCancel_DoesNotCancelMeetings()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);
        var userId = 1;

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(1),
            EndTime = from.AddHours(2),
            Canceled = false
        };

        _mockMeetingRepo.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting });

        // Act
        await _availabilityService.SetUnavailableAsync(userId, from, to, forceCancel: false);

        // Assert
        _mockMeetingService.Verify(s => s.CancelAsync(It.IsAny<CancelMeetingDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailabilityAsync_InvalidRange_ThrowsArgumentException()
    {
        // Arrange
        var from = DateTime.Now.AddDays(1);
        var to = DateTime.Now;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _availabilityService.GetAvailabilityAsync(1, from, to));
    }

    [Fact]
    public async Task GetAvailabilityAsync_ValidRange_ReturnsAvailability()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(1);
        var expectedAvailability = new List<Availability>
        {
            new Availability
            {
                CaregiverId = 1,
                StartDate = from,
                EndDate = to
            }
        };

        _mockAvailabilityRepo.Setup(r => r.GetAvailabilityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(expectedAvailability);

        // Act
        var result = await _availabilityService.GetAvailabilityAsync(1, from, to);

        // Assert
        Assert.Same(expectedAvailability, result);
        _mockAvailabilityRepo.Verify(r => r.GetAvailabilityAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverlappingMeetings_ReturnsRelevantMeetings()
    {
        // Arrange
        var from = DateTime.Now;
        var to = DateTime.Now.AddHours(5);
        var userId = 1;

        var meeting1 = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(1),
            EndTime = from.AddHours(2),
            Canceled = false
        }; // Should match
        var meeting2 = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(6),
            EndTime = from.AddHours(7),
            Canceled = false
        }; // Out of range
        var meeting3 = new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(1),
            EndTime = from.AddHours(2),
            Canceled = true
        }; // Canceled

        _mockMeetingRepo.Setup(r => r.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<Meeting> { meeting1, meeting2, meeting3 });

        // Act
        var result = await _availabilityService.GetOverlappingMeetings(userId, from, to);

        // Assert
        Assert.Single(result);
        Assert.Equal(meeting1.Id, result[0].Id);
    }
}

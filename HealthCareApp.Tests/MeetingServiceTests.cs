using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.DTOs;
using System.Text.Json;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Services.Interfaces.Notifications;

namespace HealthCareApp.Tests;

public class MeetingServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidMeeting_ReturnsSuccess()
    {
        // Arrange
        var meetingDto = new CreateMeetingDto
        {
            CaregiverId = 1,
            PatientId = 2,
            StartTime = DateTime.Now.AddHours(1),
        };
        var expectedEndTime = meetingDto.StartTime.AddMinutes(30);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedEndTime, result.Meeting!.EndTime);
        mockMeetingRepository.Verify(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>()), Times.Once);
        mockMeetingRepository.Verify(repo => repo.CreateAsync(It.IsAny<Meeting>()), Times.Once);
    }

    [Fact]
    public async Task CreateMeetingAsync_OverlappingMeeting_ReturnsFailure()
    {
        // Arrange
        var meetingDto = new CreateMeetingDto
        {
            CaregiverId = 1,
            PatientId = 2,
            StartTime = DateTime.Now.AddHours(1),
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(true);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting time unavailable", result.Message);
        mockMeetingRepository.Verify(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>()), Times.Once);
        mockMeetingRepository.Verify(repo => repo.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task CreateMeetingAsync_AdditionalSlots_ReturnsCorrectEndTime()
    {
        // Arrange
        var meetingDto = new CreateMeetingDto
        {
            CaregiverId = 1,
            PatientId = 2,
            StartTime = DateTime.Now.AddHours(1),
            Slots = 3,
        };
        var expectedEndTime = meetingDto.StartTime.AddMinutes(30 * meetingDto.Slots);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.Equal(expectedEndTime, result.Meeting!.EndTime);
    }

    [Fact]
    public async Task GetMeetingAsync_InvalidMeetingId_ReturnsNotFoundMessage()
    {
        // Arrange
        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Meeting?)null);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(Guid.NewGuid(), 1, false);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
    }

    [Fact]
    public async Task GetMeetingAsync_ValidMeetingId_UserNotParticipant_ReturnsNotFoundMessage()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;
        var nonParticipantUserId = 3;

        var meeting = new Meeting
        {
            Id = meetingId,
            Caregiver = new User { Id = caregiverId },
            Patient = new User { Id = patientId }
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, nonParticipantUserId, false);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
    }

    [Fact]
    public async Task GetMeetingAsync_ValidMeetingId_UserIsPatient_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;

        var meeting = new Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, patientId, false);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task GetMeetingAsync_ValidMeetingId_UserIsCaregiver_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;

        var meeting = new Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, caregiverId, false);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task GetMeetingAsync_ValidMeetingId_NonParticipantUser_IsAdmin_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;
        var nonParticipantUserId = 3;

        var meeting = new Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, nonParticipantUserId, true);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once());
    }

    // ConfirmAsync tests

    [Fact]
    public async Task ConfirmMeeting_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await meetingService.ConfirmAsync(null!, 1));

        // Assert
        mockMeetingRepository.Verify(r => r.GetAsync(new Guid()), Times.Never());
    }

    [Fact]
    public async Task ConfirmMeeting_InvalidId_ReturnsResultWithSuccessFalseAndMessageBookingexpired()
    {
        // Arrange
        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Meeting?)null);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);
        var meetingId = Guid.NewGuid();
        var request = new ConfirmMeetingDto { MeetingId = meetingId, PatientId = 1, Notes = "Some notes" };

        // Act
        var result = await meetingService.ConfirmAsync(request, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Booking expired", result.Message);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once());
        mockMeetingRepository.Verify(r => r.SaveChangesAsync(), Times.Never());
    }

    [Fact]
    public async Task ConfirmMeeting_PatientIdDoesNotMatch_ReturnsResultWithSuccessFalseAndMeetingNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = 1,
            Status = MeetingStatus.Pending
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);
        var request = new ConfirmMeetingDto { MeetingId = meetingId, PatientId = 2, Notes = "Some notes" }; // Mismatched PatientId

        // Act
        var result = await meetingService.ConfirmAsync(request, 2);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once());
        mockMeetingRepository.Verify(r => r.SaveChangesAsync(), Times.Never());
    }

    [Fact]
    public async Task ConfirmMeeting_StatusNotPending_ReturnsResultWithSuccessFalseAndMeetingAlreadyConfirmed()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = 1,
            Status = MeetingStatus.Confirmed
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);
        var request = new ConfirmMeetingDto { MeetingId = meetingId, PatientId = 1, Notes = "Some notes" }; // Already confirmed

        // Act
        var result = await meetingService.ConfirmAsync(request, 2);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once());
        mockMeetingRepository.Verify(r => r.SaveChangesAsync(), Times.Never());
    }

    [Fact]
    public async Task ConfirmMeeting_ValidRequest_ReturnsSuccessAndUpdatedMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = 1,
            Status = MeetingStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object);
        var request = new ConfirmMeetingDto { MeetingId = meetingId, PatientId = 1, Notes = "Some notes" }; // Already confirmed

        // Act
        var result = await meetingService.ConfirmAsync(request, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(request.Notes, result.Meeting!.Notes);
        Assert.Equal(MeetingStatus.Confirmed, result.Meeting.Status);
        Assert.Null(meeting.ExpiresAt);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once());
        mockMeetingRepository.Verify(r => r.SaveChangesAsync(), Times.Once());
    }
}

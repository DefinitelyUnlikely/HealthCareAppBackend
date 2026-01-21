using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.DTOs;
using System.Text.Json;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Services.Interfaces;
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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Roles = ["Caregiver"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Roles = ["Patient"] });
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);
        mockAvailabilityRepository.Setup(repo =>
                repo.GetAvailabilityAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Availability>
                { new Availability { StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1), CaregiverId = 1 } });

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(true);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Roles = ["Caregiver"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Roles = ["Patient"] });
        mockAvailabilityRepository.Setup(repo =>
                repo.GetAvailabilityAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Availability>
                { new Availability { StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1), CaregiverId = 1 } });

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Roles = ["Caregiver"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Roles = ["Patient"] });
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);
        mockAvailabilityRepository.Setup(repo =>
                repo.GetAvailabilityAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Availability>
                { new Availability { StartDate = DateTime.Now, EndDate = DateTime.Now.AddHours(1), CaregiverId = 1 } });

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.Equal(expectedEndTime, result.Meeting!.EndTime);
    }

    [Fact]
    public async Task CreateMeetingAsync_CaregiverBookingCaregiver_ReturnsNotSuccess()
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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Roles = ["Caregiver"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2))
            .ReturnsAsync(new User { Id = 2, Roles = ["Caregiver"] });
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("The user is not a patient", result.Message);
    }

    [Fact]
    public async Task CreateMeetingAsync_CaregiverIsNotCaregiver_ReturnsNotSuccess()
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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Roles = ["Patient"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Roles = ["Patient"] });
        mockMeetingRepository.Setup(repo => repo.CreateAsync(It.IsAny<Meeting>())).Returns(Task.CompletedTask);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("The user is not a caregiver", result.Message);
    }

    [Fact]
    public async Task GetMeetingAsync_InvalidMeetingId_ReturnsNotFoundMessage()
    {
        // Arrange
        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Meeting?)null);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Meeting?)null);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);
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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);
        var request = new ConfirmMeetingDto
        { MeetingId = meetingId, PatientId = 2, Notes = "Some notes" }; // Mismatched PatientId

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);
        var request = new ConfirmMeetingDto
        { MeetingId = meetingId, PatientId = 1, Notes = "Some notes" }; // Already confirmed

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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);
        var request = new ConfirmMeetingDto
        { MeetingId = meetingId, PatientId = 1, Notes = "Some notes" }; // Already confirmed

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

    [Fact]
    public async Task CancelAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CancelAsync(null!, 1));
    }

    [Fact]
    public async Task CancelAsync_MeetingNotFound_ReturnsError()
    {
        // Arrange
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Meeting?)null);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.CancelAsync(new CancelMeetingDto { MeetingId = Guid.NewGuid() }, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
    }

    [Fact]
    public async Task CancelAsync_UserNotPatient_ReturnsError()
    {
        // Arrange
        var meeting = new Meeting { PatientId = 1 };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.CancelAsync(new CancelMeetingDto { MeetingId = Guid.NewGuid() }, 2);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid user", result.Message);
    }

    [Fact]
    public async Task CancelAsync_MeetingNotConfirmed_ReturnsError()
    {
        // Arrange
        var meeting = new Meeting { PatientId = 1, Status = MeetingStatus.Pending };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.CancelAsync(new CancelMeetingDto { MeetingId = Guid.NewGuid() }, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Can only cancel confirmed meetings", result.Message);
    }

    [Fact]
    public async Task CancelAsync_Patient_MeetingLessThan24HoursAway_ReturnsError()
    {
        // Arrange
        int userId = 1;
        var meeting = new Meeting
        {
            PatientId = userId,
            Status = MeetingStatus.Confirmed,
            StartTime = DateTime.Now.AddHours(10)
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.CancelAsync(new CancelMeetingDto { MeetingId = Guid.NewGuid() }, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Can only cancel meetings at least 24 hours ahead", result.Message);
    }

    [Fact]
    public async Task CancelAsync_Caregiver_MeetingLessThan24HoursAway_ReturnsSuccess()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var meeting = new Meeting
        {
            Id = meetingId,
            CaregiverId = userId,
            Status = MeetingStatus.Confirmed,
            StartTime = DateTime.Now.AddHours(10),
            Canceled = false,
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.CancelAsync(new CancelMeetingDto { MeetingId = meetingId }, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(meeting.Canceled);
    }

    [Fact]
    public async Task CancelAsync_ValidRequest_CancelsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = userId,
            Status = MeetingStatus.Confirmed,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
            Canceled = false
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var request = new CancelMeetingDto { MeetingId = meetingId, Notes = "Sorry" };
        var result = await service.CancelAsync(request, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(meeting.Canceled);
        Assert.Equal("Sorry", meeting.Notes);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        UpdateMeetingDto request = null!;
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAsync(request, 1));
    }

    [Fact]
    public async Task UpdateAsync_NoExistingMeeting_ReturnsMeetingNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync((Meeting?)null);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto { MeetingId = meetingId, Notes = "" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UserNoParticipant_ReturnsMeetingNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = 2,
            CaregiverId = 3,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
            Notes = "Old notes"
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto { MeetingId = meetingId, Notes = "" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid user", result.Message);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdateTimeLessThan24HoursAway_ReturnsCorrectMessage()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var updatedStartTime = DateTime.Now.AddDays(3);
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = userId,
            StartTime = DateTime.Now.AddHours(20),
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto
        { MeetingId = meetingId, StartTime = updatedStartTime, Notes = "Less than 24h" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Can only reschedule meetings at least 24 hours ahead", result.Message);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MeetingTimeNotAvailable_ReturnsMeetingTimeUnavailable()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var updatedStartTime = DateTime.Now.AddDays(3);
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = userId,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        mockRepo.Setup(r => r.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(true);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto
        { MeetingId = meetingId, StartTime = updatedStartTime, Notes = "Invalid start time" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting time unavailable", result.Message);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatedNotes_UpdatesNotesOfExistingMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = userId,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
            Notes = "Old notes"
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto { MeetingId = meetingId, Notes = "Updated notes" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(meeting.Notes == request.Notes);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        mockRepo.Verify(r => r.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatedTime_CreatesNewMeetingAndCancelsOld()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var updatedStartTime = DateTime.Now.AddDays(3);
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = userId,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
            Canceled = false
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto
        { MeetingId = meetingId, StartTime = updatedStartTime, Notes = "Updated notes" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(meeting.Canceled);
        Assert.True(updatedStartTime == result.Meeting!.StartTime); // Make sure the new meeting is returned
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UserIsCaregiverParticipant_ReturnsSuccess()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        int userId = 1;
        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = 2,
            CaregiverId = userId,
            StartTime = DateTime.Now.AddHours(24).AddSeconds(1),
            Notes = "Old notes"
        };
        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);
        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);
        var request = new UpdateMeetingDto { MeetingId = meetingId, Notes = "" };

        // Act
        var result = await service.UpdateAsync(request, userId);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetMeetingsAsync_ReturnsMeetingDtos_WhenRepositoryReturnsMeetings()
    {
        // Arrange
        var userId = 1;
        var historic = false;
        var meetings = new List<Meeting>
        {
            new Meeting { Id = Guid.NewGuid(), PatientId = userId, StartTime = DateTime.Now.AddDays(1) },
            new Meeting { Id = Guid.NewGuid(), PatientId = userId, StartTime = DateTime.Now.AddDays(2) }
        };

        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetByUserIdAsync(userId, historic)).ReturnsAsync(meetings);

        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.GetMeetingsAsync(userId, historic);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(meetings[0].Id, result[0].Id);
        Assert.Equal(meetings[1].Id, result[1].Id);

        mockRepo.Verify(r => r.GetByUserIdAsync(userId, historic), Times.Once);
    }

    [Fact]
    public async Task GetMeetingsAsync_ReturnsEmptyList_WhenRepositoryReturnsNoMeetings()
    {
        // Arrange
        var userId = 1;
        var historic = false;
        var meetings = new List<Meeting>();

        var mockRepo = new Mock<IMeetingRepository>();
        var mockNotificationService = new Mock<INotificationService>();
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        mockRepo.Setup(r => r.GetByUserIdAsync(userId, historic)).ReturnsAsync(meetings);

        var service = new MeetingService(mockRepo.Object, mockNotificationService.Object, mockUserService.Object,
            mockAvailabilityRepository.Object);

        // Act
        var result = await service.GetMeetingsAsync(userId, historic);

        // Assert
        Assert.Empty(result);
        mockRepo.Verify(r => r.GetByUserIdAsync(userId, historic), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NoAvailability_ReturnsFailure()
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
        var mockUserService = new Mock<IUserService>();
        var mockAvailabilityRepository = new Mock<IAvailabilityRepository>();

        // Set up availability to return an empty list
        mockAvailabilityRepository.Setup(repo => repo.GetAvailabilityAsync(
                meetingDto.CaregiverId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Availability>());

        mockMeetingRepository.Setup(repo => repo.TimeUnavailableAsync(It.IsAny<Meeting>())).ReturnsAsync(false);
        mockUserService.Setup(repo => repo.GetUserByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Roles = ["Caregiver"] });
        mockUserService.Setup(repo => repo.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Roles = ["Patient"] });

        var meetingService = new MeetingService(mockMeetingRepository.Object, mockNotificationService.Object,
            mockUserService.Object, mockAvailabilityRepository.Object);

        // Act
        var result = await meetingService.CreateAsync(meetingDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting time unavailable", result.Message);
        mockAvailabilityRepository.Verify(repo => repo.GetAvailabilityAsync(
            meetingDto.CaregiverId,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>()), Times.Once);
        mockMeetingRepository.Verify(repo => repo.CreateAsync(It.IsAny<Meeting>()), Times.Never);
    }
}

using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.DTOs;
using System.Text.Json;

namespace HealthCareApp.Tests;

public class MeetingServiceTests
{
    [Fact]
    public async Task InvalidMeetingId_ReturnsNotFoundMessage()
    {
        // Arrange
        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(It.IsAny<Guid>())).ReturnsAsync((HealthCareAB_v1.Models.Meeting?)null);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(Guid.NewGuid(), 1, false);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
    }

    [Fact]
    public async Task ValidMeetingId_UserNotParticipant_ReturnsNotFoundMessage()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;
        var nonParticipantUserId = 3;

        var meeting = new HealthCareAB_v1.Models.Meeting
        {
            Id = meetingId,
            Caregiver = new HealthCareAB_v1.Models.User { Id = caregiverId },
            Patient = new HealthCareAB_v1.Models.User { Id = patientId }
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, nonParticipantUserId, false);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Meeting not found", result.Message);
    }

    [Fact]
    public async Task ValidMeetingId_UserIsPatient_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;

        var meeting = new HealthCareAB_v1.Models.Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, patientId, false);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task ValidMeetingId_UserIsCaregiver_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;

        var meeting = new HealthCareAB_v1.Models.Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, caregiverId, false);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task ValidMeetingId_NonParticipantUser_IsAdmin_ReturnsMeeting()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var caregiverId = 1;
        var patientId = 2;
        var nonParticipantUserId = 3;

        var meeting = new HealthCareAB_v1.Models.Meeting
        {
            Id = meetingId,
            CaregiverId = caregiverId,
            PatientId = patientId
        };
        var expected = MeetingResponseDto.FromEntity(meeting);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, nonParticipantUserId, true);

        // Assert
        Assert.True(result.Success);
        var expectedJson = JsonSerializer.Serialize(expected);
        var actualJson = JsonSerializer.Serialize(result);
        Assert.Equal(expectedJson, actualJson);
    }
}

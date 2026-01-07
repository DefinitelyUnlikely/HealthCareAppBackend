using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Repositories.Interfaces;

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
        var result = await meetingService.GetMeetingAsync(Guid.NewGuid(), 1);

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
        var result = await meetingService.GetMeetingAsync(meetingId, nonParticipantUserId);

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
            Caregiver = new HealthCareAB_v1.Models.User { Id = caregiverId },
            Patient = new HealthCareAB_v1.Models.User { Id = patientId }
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, patientId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(meeting, result.Meeting);
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
            Caregiver = new HealthCareAB_v1.Models.User { Id = caregiverId },
            Patient = new HealthCareAB_v1.Models.User { Id = patientId }
        };

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(repo => repo.GetAsync(meetingId)).ReturnsAsync(meeting);

        var meetingService = new MeetingService(mockMeetingRepository.Object);

        // Act
        var result = await meetingService.GetMeetingAsync(meetingId, caregiverId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(meeting, result.Meeting);
    }
}

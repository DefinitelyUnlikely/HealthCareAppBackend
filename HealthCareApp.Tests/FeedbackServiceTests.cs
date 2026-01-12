using Moq;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;

namespace HealthCareApp.Tests;

public class FeedbackServiceTests
{
    // CreateFeedbackAsync Tests

    [Fact]
    public async Task CreateFeedbackAsync_ValidFeedback_ReturnsSuccess()
    {
        // Arrange
        var patientId = 1;
        var caregiverId = 2;
        var meetingId = Guid.NewGuid();

        var feedbackDto = new CreateFeedbackDto
        {
            Rating = 5,
            Review = "Excellent service!",
            MeetingId = meetingId
        };

        var patient = new Patient
        {
            Id = patientId,
            FirstName = "John",
            LastName = "Doe",
            Roles = new List<string> { "Patient" }
        };

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            FirstName = "Dr. Jane",
            LastName = "Smith"
        };

        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = patientId,
            CaregiverId = caregiverId
        };

        var createdFeedback = new Feedback
        {
            Id = Guid.NewGuid(),
            Rating = feedbackDto.Rating,
            Review = feedbackDto.Review,
            MeetingId = meetingId,
            PatientId = patientId,
            CaregiverId = caregiverId,
            Patient = patient,
            Caregiver = caregiver
        };

        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUserByIdAsync(patientId)).ReturnsAsync(patient);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.CreateFeedbackAsync(It.IsAny<Feedback>())).ReturnsAsync(createdFeedback);

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.CreateFeedbackAsync(feedbackDto, patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(feedbackDto.Rating, result.Rating);
        Assert.Equal(feedbackDto.Review, result.Review);
        Assert.Equal(meetingId, result.MeetingId);
        mockUserService.Verify(s => s.GetUserByIdAsync(patientId), Times.Once);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockFeedbackRepository.Verify(r => r.CreateFeedbackAsync(It.IsAny<Feedback>()), Times.Once);
    }

    [Fact]
    public async Task CreateFeedbackAsync_UserNotPatient_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = 1;
        var meetingId = Guid.NewGuid();

        var feedbackDto = new CreateFeedbackDto
        {
            Rating = 5,
            Review = "Great!",
            MeetingId = meetingId
        };

        var user = new User
        {
            Id = userId,
            Roles = new List<string> { "Admin" } 
        };

        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        var mockFeedbackRepository = new Mock<IFeedbackRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await feedbackService.CreateFeedbackAsync(feedbackDto, userId)
        );

        mockUserService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        mockMeetingRepository.Verify(r => r.GetAsync(It.IsAny<Guid>()), Times.Never);
        mockFeedbackRepository.Verify(r => r.CreateFeedbackAsync(It.IsAny<Feedback>()), Times.Never);
    }

    [Fact]
    public async Task CreateFeedbackAsync_MeetingNotFound_ThrowsArgumentException()
    {
        // Arrange
        var patientId = 1;
        var meetingId = Guid.NewGuid();

        var feedbackDto = new CreateFeedbackDto
        {
            Rating = 5,
            Review = "Great!",
            MeetingId = meetingId
        };

        var patient = new Patient
        {
            Id = patientId,
            Roles = new List<string> { "Patient" }
        };

        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUserByIdAsync(patientId)).ReturnsAsync(patient);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(r => r.GetAsync(meetingId)).ReturnsAsync((Meeting?)null);

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await feedbackService.CreateFeedbackAsync(feedbackDto, patientId)
        );

        mockUserService.Verify(s => s.GetUserByIdAsync(patientId), Times.Once);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockFeedbackRepository.Verify(r => r.CreateFeedbackAsync(It.IsAny<Feedback>()), Times.Never);
    }

    [Fact]
    public async Task CreateFeedbackAsync_PatientNotPartOfMeeting_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var patientId = 1;
        var wrongPatientId = 2;
        var meetingId = Guid.NewGuid();

        var feedbackDto = new CreateFeedbackDto
        {
            Rating = 5,
            Review = "Great!",
            MeetingId = meetingId
        };

        var patient = new Patient
        {
            Id = patientId,
            Roles = new List<string> { "Patient" }
        };

        var meeting = new Meeting
        {
            Id = meetingId,
            PatientId = wrongPatientId, // Different patient
            CaregiverId = 3
        };

        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetUserByIdAsync(patientId)).ReturnsAsync(patient);

        var mockMeetingRepository = new Mock<IMeetingRepository>();
        mockMeetingRepository.Setup(r => r.GetAsync(meetingId)).ReturnsAsync(meeting);

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await feedbackService.CreateFeedbackAsync(feedbackDto, patientId)
        );

        mockUserService.Verify(s => s.GetUserByIdAsync(patientId), Times.Once);
        mockMeetingRepository.Verify(r => r.GetAsync(meetingId), Times.Once);
        mockFeedbackRepository.Verify(r => r.CreateFeedbackAsync(It.IsAny<Feedback>()), Times.Never);
    }

    // GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsFeedback()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var feedback = new Feedback
        {
            Id = feedbackId,
            Rating = 5,
            Review = "Excellent!",
            MeetingId = Guid.NewGuid(),
            PatientId = 1,  
            CaregiverId = 2,  
            Patient = new Patient { FirstName = "John", LastName = "Doe" },
            Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
        };

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(feedback);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.GetByIdAsync(feedbackId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(feedbackId, result.Id);
        Assert.Equal(feedback.Rating, result.Rating);
        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync((Feedback?)null);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.GetByIdAsync(feedbackId);

        // Assert
        Assert.Null(result);
        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
    }

    // GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsFeedbackList()
    {
        // Arrange
        var feedbacks = new List<Feedback>
        {
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 5,
                Review = "Great!",
                MeetingId = Guid.NewGuid(),
                PatientId = 1,  
                CaregiverId = 2,  
                Patient = new Patient { FirstName = "John", LastName = "Doe" },
                Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 4,
                Review = "Good!",
                MeetingId = Guid.NewGuid(),
                PatientId = 3,  
                CaregiverId = 4,  
                Patient = new Patient { FirstName = "Alice", LastName = "Johnson" },
                Caregiver = new Caregiver { FirstName = "Dr. Bob", LastName = "Brown" }
            }
        };

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(feedbacks);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockFeedbackRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    // UpdateFeedbackAsync Tests

    [Fact]
    public async Task UpdateFeedbackAsync_ValidUpdate_ReturnsUpdatedFeedback()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var patientId = 1;

        var updateDto = new UpdateFeedbackDto
        {
            Rating = 4,
            Review = "Updated review"
        };

        var existingFeedback = new Feedback
        {
            Id = feedbackId,
            Rating = 5,
            Review = "Original review",
            MeetingId = Guid.NewGuid(),
            PatientId = patientId,
            CaregiverId = 2,  
            Patient = new Patient { Id = patientId, FirstName = "John", LastName = "Doe" },
            Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
        };

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(existingFeedback);
        mockFeedbackRepository.Setup(r => r.UpdateAsync(It.IsAny<Feedback>())).ReturnsAsync(existingFeedback);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.UpdateFeedbackAsync(feedbackId, updateDto, patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Rating, result.Rating);
        Assert.Equal(updateDto.Review, result.Review);
        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
        mockFeedbackRepository.Verify(r => r.UpdateAsync(It.IsAny<Feedback>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_FeedbackNotFound_ReturnsNull()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var patientId = 1;

        var updateDto = new UpdateFeedbackDto
        {
            Rating = 4,
            Review = "Updated review"
        };

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync((Feedback?)null);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.UpdateFeedbackAsync(feedbackId, updateDto, patientId);

        // Assert
        Assert.Null(result);
        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
        mockFeedbackRepository.Verify(r => r.UpdateAsync(It.IsAny<Feedback>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFeedbackAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var patientId = 1;
        var wrongUserId = 2;

        var updateDto = new UpdateFeedbackDto
        {
            Rating = 4,
            Review = "Updated review"
        };

        var existingFeedback = new Feedback
        {
            Id = feedbackId,
            Rating = 5,
            Review = "Original review",
            MeetingId = Guid.NewGuid(),
            PatientId = patientId,
            CaregiverId = 3,  
            Patient = new Patient { FirstName = "John", LastName = "Doe" },
            Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
        };

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(existingFeedback);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await feedbackService.UpdateFeedbackAsync(feedbackId, updateDto, wrongUserId)
        );

        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
        mockFeedbackRepository.Verify(r => r.UpdateAsync(It.IsAny<Feedback>()), Times.Never);
    }


[Fact]
public async Task DeleteFeedbackAsync_ValidDelete_ReturnsTrue()
{
    // Arrange
    var feedbackId = Guid.NewGuid();
    var patientId = 1;

    var existingFeedback = new Feedback
    {
        Id = feedbackId,
        Rating = 5,  
        MeetingId = Guid.NewGuid(),
        PatientId = patientId,
        CaregiverId = 2,
        Patient = new Patient { Id = patientId, Roles = new List<string> { "Patient" } },
        Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
    };

    var mockFeedbackRepository = new Mock<IFeedbackRepository>();
    mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(existingFeedback);
    mockFeedbackRepository.Setup(r => r.DeleteAsync(feedbackId)).ReturnsAsync(true);

    var mockUserService = new Mock<IUserService>();
    mockUserService.Setup(s => s.GetUserByIdAsync(patientId)).ReturnsAsync(existingFeedback.Patient);

    var mockMeetingRepository = new Mock<IMeetingRepository>();

    var feedbackService = new FeedbackService(
        mockFeedbackRepository.Object,
        mockUserService.Object,
        mockMeetingRepository.Object
    );

    // Act
    var result = await feedbackService.DeleteFeedbackAsync(feedbackId, patientId);

    // Assert
    Assert.True(result);
    mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
    mockFeedbackRepository.Verify(r => r.DeleteAsync(feedbackId), Times.Once);
}

    [Fact]
    public async Task DeleteFeedbackAsync_FeedbackNotFound_ReturnsFalse()
    {
        // Arrange
        var feedbackId = Guid.NewGuid();
        var patientId = 1;

        var mockFeedbackRepository = new Mock<IFeedbackRepository>();
        mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync((Feedback?)null);

        var mockUserService = new Mock<IUserService>();
        var mockMeetingRepository = new Mock<IMeetingRepository>();

        var feedbackService = new FeedbackService(
            mockFeedbackRepository.Object,
            mockUserService.Object,
            mockMeetingRepository.Object
        );

        // Act
        var result = await feedbackService.DeleteFeedbackAsync(feedbackId, patientId);

        // Assert
        Assert.False(result);
        mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
        mockFeedbackRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

[Fact]
public async Task DeleteFeedbackAsync_UserNotOwner_ThrowsUnauthorizedAccessException()
{
    // Arrange
    var feedbackId = Guid.NewGuid();
    var patientId = 1;
    var wrongUserId = 2;

    var existingFeedback = new Feedback
    {
        Id = feedbackId,
        Rating = 5,  
        MeetingId = Guid.NewGuid(),
        PatientId = patientId,
        CaregiverId = 3,
        Patient = new Patient { FirstName = "John", LastName = "Doe" },
        Caregiver = new Caregiver { FirstName = "Dr. Jane", LastName = "Smith" }
    };

    var wrongUser = new User
    {
        Id = wrongUserId,
        Roles = new List<string> { "Patient" }
    };

    var mockFeedbackRepository = new Mock<IFeedbackRepository>();
    mockFeedbackRepository.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(existingFeedback);

    var mockUserService = new Mock<IUserService>();
    mockUserService.Setup(s => s.GetUserByIdAsync(wrongUserId)).ReturnsAsync(wrongUser);

    var mockMeetingRepository = new Mock<IMeetingRepository>();

    var feedbackService = new FeedbackService(
        mockFeedbackRepository.Object,
        mockUserService.Object,
        mockMeetingRepository.Object
    );

    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        async () => await feedbackService.DeleteFeedbackAsync(feedbackId, wrongUserId)
    );

    mockFeedbackRepository.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
    mockFeedbackRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
}

}
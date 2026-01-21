using HealthCareAB_v1.Models;
using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Implementations.Notifications;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;
using HealthCareAB_v1.Exceptions;
using Moq;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;

namespace HealthCareApp.Tests;

public static class TestData
{
    public static User GetMockPatient()
    {
        return new User
        {
            Id = 1,
            Username = "patient1",
            Email = "patient@example.com",
            FirstName = "Patient",
            LastName = "Patientsson",
            Roles = ["Patient"],
            Address = "Patientgatan 1",
            PasswordHash = "hashed_password",
            PersonalNumber = "19900101-1234",
            PhoneNumber = "1234567890"
        };
    }

    public static User GetMockCaregiver()
    {
        return new User
        {
            Id = 2,
            Username = "caregiver1",
            Email = "caregiver@example.com",
            FirstName = "Doktor",
            LastName = "Alban",
            Roles = ["Caregiver"],
            Address = "Vårdgivargatan 1",
            PasswordHash = "hashed_password",
            PersonalNumber = "19850505-5678",
            PhoneNumber = "0987654321"
        };
    }

    public static Meeting GetMockMeeting()
    {
        return new Meeting
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(1).AddHours(1),
            Patient = GetMockPatient(),
            Caregiver = GetMockCaregiver(),
            Status = MeetingStatus.Confirmed,
            Canceled = false,
            Notes = "Vanligt möte"
        };
    }
}

public class NotificationModelEmailNotificationTests
{
    [Fact]
    public void MeetingConfirmedEmailNotification_HasExpectedValues_WhenCaregiverExists()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();

        // Act
        var notification = new MeetingConfirmedEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte bokat " + meeting.StartTime + " på " + meeting.Caregiver!.Address + ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte bokat {meeting.StartTime} på {meeting.Caregiver.Address}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingConfirmedEmailNotification_HasExpectedValues_WhenCaregiverIsNull()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        meeting.Caregiver = null;

        // Act
        var notification = new MeetingConfirmedEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte bokat " + meeting.StartTime + ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte bokat {meeting.StartTime}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingCancelledEmailNotification_HasExpectedValues_WhenCaregiverExists()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();

        // Act
        var notification = new MeetingCancelledEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte vid " + meeting.StartTime + " på " + meeting.Caregiver!.Address +
            " avbokat. Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte vid {meeting.StartTime} på {meeting.Caregiver.Address} avbokat</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingCancelledEmailNotification_HasExpectedValues_WhenCaregiverIsNull()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        meeting.Caregiver = null;

        // Act
        var notification = new MeetingCancelledEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte vid " + meeting.StartTime + " avbokat. Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte vid {meeting.StartTime} avbokat</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingReminderEmailNotification_HasExpectedValues_WhenCaregiverExists()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        meeting.Canceled = true;

        // Act
        var notification = new MeetingReminderEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Mötespåminnelse för möte vid " + meeting.StartTime + " på " + meeting.Caregiver!.Address +
            ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Mötespåminnelse för möte vid {meeting.StartTime} på {meeting.Caregiver.Address}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingReminderEmailNotification_HasExpectedValues_WhenCaregiverIsNull()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        meeting.Canceled = true;
        meeting.Caregiver = null;

        // Act
        var notification = new MeetingReminderEmailNotification()
        {
            Meeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(meeting, notification.Meeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Mötespåminnelse för möte vid " + meeting.StartTime + ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Mötespåminnelse för möte vid {meeting.StartTime}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingUpdatedEmailNotification_HasExpectedValues_WhenCaregiverExists()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        var oldMeeting = TestData.GetMockMeeting();
        oldMeeting.StartTime = DateTime.Now;
        oldMeeting.EndTime = DateTime.Now.AddHours(1);
        oldMeeting.Notes = "Gamla anteckningar";

        // Act
        var notification = new MeetingUpdatedEmailNotification()
        {
            OldMeeting = oldMeeting,
            NewMeeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(oldMeeting, notification.OldMeeting);
        Assert.Equal(meeting, notification.NewMeeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte uppdaterat från " + oldMeeting.StartTime + " till " + meeting.StartTime + " på " +
            meeting.Caregiver!.Address +
            ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte uppdaterat från {oldMeeting.StartTime} till {meeting.StartTime} på {meeting.Caregiver.Address}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }

    [Fact]
    public void MeetingUpdatedEmailNotification_HasExpectedValues_WhenCaregiverIsNull()
    {
        // Arrange
        var meeting = TestData.GetMockMeeting();
        meeting.Caregiver = null;

        var oldMeeting = TestData.GetMockMeeting();
        oldMeeting.StartTime = DateTime.Now;
        oldMeeting.EndTime = DateTime.Now.AddHours(1);
        oldMeeting.Notes = "Gamla anteckningar";

        // Act
        var notification = new MeetingUpdatedEmailNotification()
        {
            OldMeeting = oldMeeting,
            NewMeeting = meeting,
            Subject = "Subject",
            Html = "Html",
            Message = "Message",
            RecipientUser = meeting.Patient!
        };

        // Assert
        Assert.Equal(oldMeeting, notification.OldMeeting);
        Assert.Equal(meeting, notification.NewMeeting);
        Assert.Equal(meeting.Patient!.Email, notification.RecipientUser.Email);
        Assert.Equal(
            "Hej, Möte uppdaterat från " + oldMeeting.StartTime + " till " + meeting.StartTime +
            ". Hälsningar HealthCare AB",
            notification.Message);
        Assert.Equal(
            $"<p>Hej,</p><p>Möte uppdaterat från {oldMeeting.StartTime} till {meeting.StartTime}</p><p>Hälsningar HealthCare AB</p>",
            notification.Html);
    }
}

public class NotificationServiceTests
{
    [Fact]
    public async Task SendNotificationAsync_WhenNotificationIsEmailNotification_CallsEmailHandler()
    {
        // Arrange
        var notification = new MeetingConfirmedEmailNotification
        {
            Meeting = TestData.GetMockMeeting(),
            RecipientUser = TestData.GetMockPatient(),
            Subject = "Test Subject"
        };
        var handlerMock = new Mock<INotificationHandler>();
        handlerMock.Setup(h => h.CanHandle(notification)).Returns(true);
        var service = new NotificationService([handlerMock.Object]);

        // Act
        await service.SendNotificationAsync(notification);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(notification), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsHandlerNotFoundException_WhenNoHandlers()
    {
        // Arrange
        var notification = new MeetingConfirmedEmailNotification
        {
            Meeting = TestData.GetMockMeeting(),
            RecipientUser = TestData.GetMockPatient(),
            Subject = "Test Subject"
        };
        var service = new NotificationService(Enumerable.Empty<INotificationHandler>());

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => service.SendNotificationAsync(notification));
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsHandlerNotFoundException_WhenNoHandlersForNotificationType()
    {
        // Arrange
        var notification = new MeetingConfirmedEmailNotification
        {
            Meeting = TestData.GetMockMeeting(),
            RecipientUser = TestData.GetMockPatient(),
            Subject = "Test Subject"
        };
        var handlerMock = new Mock<INotificationHandler>();
        handlerMock.Setup(h => h.CanHandle(notification)).Returns(false);
        var service = new NotificationService([handlerMock.Object]);

        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => service.SendNotificationAsync(notification));
    }

    // In the future, if we add more notification types with handlers, we need to add tests for those as well. 
    // we'll also need to add a test that checks that if a notification type has multiple handlers, all of them are called.
}

public class NotificationHandlerTests
{
    [Fact]
    public void EmailNotificationHandler_CanHandle_ReturnsTrueForEmailNotification()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var handler = new EmailNotificationHandler(emailServiceMock.Object);
        var notification = new MeetingConfirmedEmailNotification
        {
            Meeting = TestData.GetMockMeeting(),
            RecipientUser = TestData.GetMockPatient(),
            Subject = "Test Subject"
        };

        // Act
        var result = handler.CanHandle(notification);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EmailNotificationHandler_CanHandle_ReturnsFalseForOtherNotification()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var handler = new EmailNotificationHandler(emailServiceMock.Object);
        var notification = new Notification { RecipientUser = TestData.GetMockPatient() };

        // Act
        var result = handler.CanHandle(notification);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailNotificationHandler_HandleAsync_CallsEmailService_WhenNotificationIsEmailNotification()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var handler = new EmailNotificationHandler(emailServiceMock.Object);
        var notification = new MeetingConfirmedEmailNotification
        {
            Meeting = TestData.GetMockMeeting(),
            RecipientUser = TestData.GetMockPatient(),
            Subject = "Test Subject"
        };

        // Act
        await handler.HandleAsync(notification);

        // Assert
        emailServiceMock.Verify(s => s.SendEmailAsync(It.Is<IEmailService.Email>(email =>
            email.To == notification.RecipientUser.Email &&
            email.Subject == notification.Subject &&
            email.HtmlContent == notification.Html &&
            email.PlainContent == notification.Message
        )), Times.Once);
    }
}

public class MeetingNotificationTests
{
    [Fact]
    public async Task ConfirmAsync_SendsNotification_WhenPatientExists()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        meeting.PatientId = meeting.Patient!.Id;
        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };
        var userId = meeting.Patient.Id;

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<MeetingConfirmedEmailNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_DoesNotSendNotification_WhenPatientIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient = null;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_DoesNotSendNotification_WhenPatientEmailIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient.Email = null!;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SendsNotification_WhenPatientExists()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        meeting.PatientId = meeting.Patient!.Id;
        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };
        var userId = meeting.Patient.Id;

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<MeetingConfirmedEmailNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotSendNotification_WhenPatientIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient = null;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotSendNotification_WhenPatientEmailIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient!.Email = null!;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_SendsNotification_WhenPatientExists()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        meeting.PatientId = meeting.Patient!.Id;
        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };
        var userId = meeting.Patient.Id;

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<MeetingConfirmedEmailNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_DoesNotSendNotification_WhenPatientIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient = null;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_DoesNotSendNotification_WhenPatientEmailIsNull()
    {
        // Arrange
        var meetingRepoMock = new Mock<IMeetingRepository>();
        var notificationServiceMock = new Mock<INotificationService>();
        var userServiceMock = new Mock<IUserService>();
        var availabilityServiceMock = new Mock<IAvailabilityService>();
        var meeting = TestData.GetMockMeeting();
        meeting.Status = MeetingStatus.Pending;
        var userId = meeting.Patient!.Id;
        meeting.PatientId = userId;

        meetingRepoMock.Setup(r => r.GetAsync(meeting.Id)).ReturnsAsync(meeting);

        var service = new MeetingService(meetingRepoMock.Object, notificationServiceMock.Object, userServiceMock.Object,
            availabilityServiceMock.Object);
        meeting.Patient!.Email = null!;

        var request = new ConfirmMeetingDto { MeetingId = meeting.Id, Notes = "Confirmed" };

        // Act
        await service.ConfirmAsync(request, userId);

        // Assert
        notificationServiceMock.Verify(n => n.SendNotificationAsync(It.IsAny<Notification>()), Times.Never);
    }
}

namespace HealthCareApp.Tests;


public class NotificationModel_EmailNotification_Tests
{
    [Fact]
    public void MeetingConfirmedEmailNotification_HasExpectedValues()
    {
    }

    [Fact]
    public void MeetingCancelledEmailNotification_HasExpectedValues()
    {
    }

    [Fact]
    public void MeetingReminderEmailNotification_HasExpectedValues()
    {
    }

    [Fact]
    public void MeetingUpdatedEmailNotification_HasExpectedValues()
    {
    }
}

public class NotificationServiceTests
{
    [Fact]
    public async Task SendNotificationAsync_WhenNotificationIsEmailNotification_CallsEmailHandler()
    {
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsHandlerNotFoundException_WhenNoHandlers()
    {
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsHandlerNotFoundException_WhenNoHandlersForNotificationType()
    {
    }

    // In the future, if we add more notification types with handlers, we need to add tests for those as well. 
    // we'll also need to add a test that checks that if a notification type has multiple handlers, all of them are called.
}

public class NotificationHandlerTests
{
    [Fact]
    public async Task EmailNotificationHandler_CanHandle_ReturnsTrueForEmailNotification()
    {
    }

    [Fact]
    public async Task EmailNotificationHandler_CanHandle_ReturnsFalseForOtherNotification()
    {
    }

    [Fact]
    public async Task EmailNotificationHandler_HandleAsync_CallsEmailService_WhenNotificationIsEmailNotification()
    {
    }
}
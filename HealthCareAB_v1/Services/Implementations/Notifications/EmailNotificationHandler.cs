using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;

namespace HealthCareAB_v1.Services.Implementations.Notifications;

public class EmailNotificationHandler(IEmailService emailService) : INotificationHandler
{
    public async Task HandleAsync(Notification notification)
    {
        if (notification is not EmailNotification emailNotification)
        {
            return;
        }

        var email = new IEmailService.Email
        {
            To = emailNotification.RecipientUser.Email,
            Subject = emailNotification.Subject,
            HtmlContent = emailNotification.Html,
            PlainContent = emailNotification.Message
        };
        await emailService.SendEmailAsync(email);
    }

    public bool CanHandle(Notification notification)
    {
        return notification is EmailNotification;
    }
}
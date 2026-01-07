using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;

namespace HealthCareAB_v1.Services.Implementations.Notifications;

public class EmailNotificationHandler(IEmailService emailService) : INotificationHandler<EmailNotification>
{
    public async Task HandleAsync(EmailNotification notification)
    {
        var email = new IEmailService.Email
        {
            To = notification.SendToUser.Email,
            Subject = notification.Subject,
            HtmlContent = notification.Html,
            PlainContent = notification.Message
        };
        await emailService.SendEmailAsync(email);
    }
}
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.DTOs.Notification;

namespace HealthCareAB_v1.Services.Implementations;

public class EmailNotificationService(ILogger<EmailNotificationService> logger, IEmailService emailService)
    : INotificationService
{
    public async Task SendNotificationAsync(Notification notification)
    {
        if (notification is not MeetingEmailNotification emailNotification)
        {
            throw new ArgumentException("Notification is not an email notification");
        }

        logger.LogInformation("Sending email notification of type {NotificationType}",
            emailNotification.GetType().Name);
        await emailService.SendEmailAsync(new IEmailService.Email
        {
            To = emailNotification.To,
            Subject = emailNotification.Subject,
            PlainContent = emailNotification.Message,
            HtmlContent = emailNotification.HtmlMessage
        });
    }
}
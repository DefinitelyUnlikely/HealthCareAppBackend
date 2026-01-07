using HealthCareAB_v1.Models.Notification;

namespace HealthCareAB_v1.Services.Interfaces.Notifications;

// But how do I make sure the correct handler is used? 
public interface INotificationHandler
{
    Task HandleAsync(Notification notification);
}
using HealthCareAB_v1.Models.Notification;

namespace HealthCareAB_v1.Services.Interfaces.Notifications;

// But how do I make sure the correct handler is used?
// Generics? Hmm, perhaps?
public interface INotificationHandler<in TNotification> where TNotification : Notification
{
    Task HandleAsync(TNotification notification);
}
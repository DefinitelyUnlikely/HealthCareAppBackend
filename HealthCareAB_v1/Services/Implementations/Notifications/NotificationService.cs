using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Interfaces.Notifications;

namespace HealthCareAB_v1.Services.Implementations.Notifications;

public class NotificationService : INotificationService
{
    public Task SendNotificationAsync(Notification notification)
    {
        // What needs to be done here?
        // 1. Check through all handlers, to see if any of them can handle the notification
        // 2. If a handler can handle the notification, call the handler
        // 3. If no handler can handle the notification, throw an exception
        throw new NotImplementedException();
    }
}

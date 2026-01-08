using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Interfaces.Notifications;
using HealthCareAB_v1.Exceptions;

namespace HealthCareAB_v1.Services.Implementations.Notifications;

public class NotificationService(IEnumerable<INotificationHandler> handlers) : INotificationService
{
    public async Task SendNotificationAsync(Notification notification)
    {
        if (handlers is null)
        {
            throw new HandlerNotFoundException("No handlers found.");
        }

        var handlerCount = 0;
        foreach (var handler in handlers)
        {
            if (!handler.CanHandle(notification)) continue;
            handlerCount++;
            await handler.HandleAsync(notification);
        }

        if (handlerCount == 0)
        {
            throw new HandlerNotFoundException("No handlers found for notification.");
        }
    }
}

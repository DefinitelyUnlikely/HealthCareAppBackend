using HealthCareAB_v1.Models.Notification;

namespace HealthCareAB_v1.Services.Interfaces.Notifications;

public interface INotificationService
{
    Task SendNotificationAsync(Notification notification);
}

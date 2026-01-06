using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(Notification notification);
}
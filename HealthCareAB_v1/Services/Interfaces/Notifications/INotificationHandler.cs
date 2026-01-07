using HealthCareAB_v1.Models.Notification;

namespace HealthCareAB_v1.Services.Interfaces.Notifications;

// But how do I make sure the correct handler is used?
// Generics? Hmm, perhaps? 
public interface INotificationHandler<in TNotification> where TNotification : Notification
{
    Task HandleAsync(TNotification notification);

    // Well, There is probably a way to do this with reflections 
    // but I do not know enough about that to make that happen right now. 
    // I googled get type of service and there is such a thing as IServiceProvider.GetService
    // that gets us the service based on the type. So... If I can figure out how to get the type of the 
    // service some other way than throug this method, we can use that to get the type, then get the service (our handler)
    // and then call the HandleAsync method. But this should work for now.
    bool CanHandle(Notification notification);
}
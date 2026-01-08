using HealthCareAB_v1.Models.Notification;

namespace HealthCareAB_v1.Services.Interfaces.Notifications;

// But how do I make sure the correct handler is used?
// Generics? Hmm, perhaps? 

// Well, There is probably a way to do this with reflections 
// but I do not know enough about that to make that happen right now. 
// I googled get type of service and there is such a thing as IServiceProvider.GetService
// that gets us the service based on the type. So... If I can figure out how to get the type of the 
// service some other way than throug this method, we can use that to get the type, then get the service (our handler)
// and then call the HandleAsync method. But this should work for now.

// In the end, I tried to make it work with generics and all that, but I could not get it to work.
// So I ended up with the simple solution I had before. CanHandle will deal with type safety and 
// apparently, if you inject an IEnumerable<type>, it will inject an enumerable with all the services of that type.
// So we can just loop through them and check if they can handle the notification.
public interface INotificationHandler
{
    Task HandleAsync(Notification notification);
    bool CanHandle(Notification notification);
}

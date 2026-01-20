using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using HealthCareAB_v1.Services.Interfaces.Notifications;
using HealthCareAB_v1.Repositories.Interfaces;

namespace Functions.Timed
{
    public class DailyTaskFunction
    {
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IAppDbContext _dbContext;

        public DailyTaskFunction(ILoggerFactory loggerFactory, INotificationService notificationService, IAppDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<DailyTaskFunction>();
            _notificationService = notificationService;
            _dbContext = dbContext;
        }

        [Function("ReminderNotificationFunction")]
        public void Run([TimerTrigger("0 0 7 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
            
            // We'd be calling the notification service here, but I am not going to set up a blob storage 
            // etc for this example, so we're leaving it empty and just as a placeholder to show the idea.

        }
    }
}

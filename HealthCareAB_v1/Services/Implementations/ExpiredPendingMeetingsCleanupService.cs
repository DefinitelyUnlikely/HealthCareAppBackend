using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HealthCareAB_v1.Services.Implementations;

public class ExpiredPendingMeetingsCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExpiredPendingMeetingsCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IMeetingRepository>();

            await repo.DeleteExpiredPendingMeetingsAsync();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

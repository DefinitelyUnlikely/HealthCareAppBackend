using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IMeetingRepository
{
    Task CreateAsync(Meeting meeting);
    Task<Meeting?> GetAsync(Guid id);
    Task<List<Meeting>> GetAllAsync(DateTime from, DateTime to);
    Task<List<Meeting>> GetByUserIdAsync(int userId, bool includeHistoric);
    Task<bool> TimeUnavailableAsync(Meeting meeting);
    Task<int> SaveChangesAsync();

    Task<int> DeleteExpiredPendingMeetingsAsync();
}

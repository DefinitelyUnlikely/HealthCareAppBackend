using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IMeetingRepository
{
    public Task CreateAsync(Meeting meeting);
    public Task<Meeting?> GetAsync(Guid id);
    public Task<bool> TimeUnavailableAsync(Meeting meeting);
    public Task<int> SaveChangesAsync();
}

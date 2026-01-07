using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IMeetingRepository
{
    public Task<Meeting?> GetAsync(Guid id);
}

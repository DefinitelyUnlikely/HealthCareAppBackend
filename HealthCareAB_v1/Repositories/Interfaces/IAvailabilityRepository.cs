using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAvailabilityRepository
{
    public Task SaveUnavailabilityAsync(Availability availability);
    public Task<List<Availability>> GetUnavailabilityAsync(int userId, DateTime? from, DateTime? to);
    public Task DeleteUnavailabilityAsync(int userId, DateTime? from, DateTime? to);
}
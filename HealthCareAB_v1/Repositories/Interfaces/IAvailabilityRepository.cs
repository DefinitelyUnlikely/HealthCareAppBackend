using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAvailabilityRepository
{
    public Task SaveAvailabilityAsync(Availability availability);
    public Task<List<Availability>> GetAvailabilityAsync(int? userId, DateTime from, DateTime to);
    public Task DeleteAvailabilityAsync(int userId, DateTime from, DateTime to);
}
using HealthCareAB_v1.Repositories.Interfaces;

namespace HealthCareAB_v1.Repositories.Implementations;

public class AvailabilityRepository : IAvailabilityRepository
{
    public Task SaveAvailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        throw new NotImplementedException();
    }

    public Task GetAvailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        throw new NotImplementedException();
    }
}

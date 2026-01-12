using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class AvailabilityService : IAvailabilityService
{
    public Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        throw new NotImplementedException();
    }

    public Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null, bool forceCancel = false)
    {
        throw new NotImplementedException();
    }

    public Task GetAvailabilityAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        throw new NotImplementedException();
    }
}
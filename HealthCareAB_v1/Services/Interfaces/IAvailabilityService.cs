using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IAvailabilityService
{
    public Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null);
    public Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null, bool forceCancel = false);

    public Task<List<Availability>> GetAvailabilityAsync(int userId, DateTime? from = null, DateTime? to = null);
}
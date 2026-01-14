using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class AvailabilityService(IAvailabilityRepository availabilityRepository) : IAvailabilityService
{
    // Assuming we are available by default, this method, somewhat ironically,
    // calls the delete operation from the repository. 
    public Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        throw new NotImplementedException();
    }

    public Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null, bool forceCancel = false)
    {
        throw new NotImplementedException();
    }

    public Task GetUnavailabilityAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        return availabilityRepository.GetUnavailabilityAsync(userId, from, to);
    }
}
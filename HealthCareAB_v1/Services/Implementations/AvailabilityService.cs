using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class AvailabilityService(IAvailabilityRepository availabilityRepository) : IAvailabilityService
{
    // Assuming we are available by default, this method, somewhat ironically,
    // calls the delete operation from the repository. 
    public async Task SetAvailableAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        throw new NotImplementedException();
    }

    public async Task SetUnavailableAsync(int userId, DateTime? from = null, DateTime? to = null,
        bool forceCancel = false)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Availability>> GetUnavailabilityAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        return await availabilityRepository.GetUnavailabilityAsync(userId, from ?? DateTime.Now,
            to ?? DateTime.Now.AddMonths(3));
    }
}
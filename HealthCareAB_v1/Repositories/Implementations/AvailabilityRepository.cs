using HealthCareAB_v1.Repositories.Interfaces;

namespace HealthCareAB_v1.Repositories.Implementations;

public class AvailabilityRepository(IAppDbContext context) : IAvailabilityRepository
{
    private readonly IAppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task SaveAvailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        throw new NotImplementedException();
    }

    public Task GetAvailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        // How do I figure this one to work?
        // My first thought is simply taking the dates and 
        // then creating a query that returns all availabilities.
        // We add a where statement to only return entitites that have start and end dates 
        // within the range of the dates provided.
        // and another where statement to only return entities that have the user id provided.
        throw new NotImplementedException();
    }
}

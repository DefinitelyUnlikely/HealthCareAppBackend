using HealthCareAB_v1.Repositories.Interfaces;

namespace HealthCareAB_v1.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly IAppDbContext _context;

    public MeetingRepository(IAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
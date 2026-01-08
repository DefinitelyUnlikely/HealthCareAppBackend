using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly IAppDbContext _context;

    public MeetingRepository(IAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Meeting?> GetAsync(Guid id)
    {
        return await _context.Meetings.FindAsync(id);
    }
}

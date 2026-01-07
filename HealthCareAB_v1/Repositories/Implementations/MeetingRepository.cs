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

    public async Task<Meeting?> CreateAsync(Guid id)
    {
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();
        return meeting;
    }
    public async Task<Meeting?> GetAsync(Guid id)
    {
        return await _context.Meetings.FindAsync(id);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

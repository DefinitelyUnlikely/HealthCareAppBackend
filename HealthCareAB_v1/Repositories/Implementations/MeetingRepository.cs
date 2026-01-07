using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models;
using Microsoft.EntityFrameworkCore;


namespace HealthCareAB_v1.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly IAppDbContext _context;

    public MeetingRepository(IAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<Meeting> CreateAsync(Meeting meeting)
    {
        await _context.Meetings.AddAsync(meeting);
        await SaveChangesAsync();
        return meeting;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

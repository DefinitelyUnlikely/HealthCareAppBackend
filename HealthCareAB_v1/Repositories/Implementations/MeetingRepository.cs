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

    public async Task CreateAsync(Meeting meeting)
    {
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task<Meeting?> GetAsync(Guid id)
    {
        return await _context.Meetings
            .Include(m => m.Caregiver)
            .Include(m => m.Patient)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Meeting>> GetAllAsync(DateTime from, DateTime to)
    {
        return await _context.Meetings
            .Include(m => m.Caregiver)
            .Include(m => m.Patient)
            .Where(m => m.StartTime >= from && m.EndTime <= to)
            .ToListAsync();
    }

    public async Task<List<Meeting>> GetByUserIdAsync(int userId, bool includeHistoric)
    {
        var query = _context.Meetings
            .Include(m => m.Caregiver)
            .Include(m => m.Patient)
            .Where(m => m.PatientId == userId || m.CaregiverId == userId);

        if (!includeHistoric)
        {
            query = query.Where(m => m.StartTime > DateTime.UtcNow);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> TimeUnavailableAsync(Meeting meeting)
    {
        return await _context.Meetings.AnyAsync(m =>
            m.CaregiverId == meeting.CaregiverId && m.StartTime < meeting.EndTime && m.EndTime > meeting.StartTime);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteExpiredPendingMeetingsAsync()
    {
    var cutoff = DateTime.UtcNow.AddMinutes(-15);

    var expired = await _context.Meetings
        .Where(m =>
            m.Status == MeetingStatus.Pending &&
            m.ExpiresAt != null &&
            m.ExpiresAt <= cutoff)
        .ToListAsync();

    if (expired.Count == 0) return 0;

    _context.Meetings.RemoveRange(expired);
    await _context.SaveChangesAsync();
    return expired.Count;
    }
}
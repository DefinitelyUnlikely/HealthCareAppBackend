using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class AvailabilityRepository(IAppDbContext context) : IAvailabilityRepository
{
    private readonly IAppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task SaveAvailabilityAsync(Availability availability)
    {
        await _context.Availabilities.AddAsync(availability);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Availability>> GetAvailabilityAsync(int? userId, DateTime? from, DateTime? to)
    {
        if (userId == null)
        {
            return await _context.Availabilities
                .Where(a => a.StartTime <= to && a.EndTime >= from)
                .ToListAsync();
        }

        return await _context.Availabilities
            .Where(a => a.CaregiverId == userId && a.StartTime <= to && a.EndTime >= from)
            .ToListAsync();
    }

    public async Task DeleteAvailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        var availability = await _context.Availabilities
            .Where(a => a.CaregiverId == userId && a.StartTime <= to && a.EndTime >= from)
            .ToListAsync();

        _context.Availabilities.RemoveRange(availability);
        await _context.SaveChangesAsync();
    }
}
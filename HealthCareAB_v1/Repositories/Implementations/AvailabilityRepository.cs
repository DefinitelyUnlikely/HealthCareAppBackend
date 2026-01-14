using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class AvailabilityRepository(IAppDbContext context) : IAvailabilityRepository
{
    private readonly IAppDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task SaveUnavailabilityAsync(Unavailability availability)
    {
        await _context.Unavailabilities.AddAsync(availability);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Unavailability>> GetUnavailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        return await _context.Unavailabilities
            .Where(a => a.CaregiverId == userId && a.StartDate <= to && a.EndDate >= from)
            .ToListAsync();
    }

    public async Task DeleteUnavailabilityAsync(int userId, DateTime? from, DateTime? to)
    {
        var availability = await _context.Unavailabilities
            .Where(a => a.CaregiverId == userId && a.StartDate <= to && a.EndDate >= from)
            .ToListAsync();

        if (availability == null)
        {
            throw new ArgumentException("Availability not found");
        }

        _context.Unavailabilities.RemoveRange(availability);
        await _context.SaveChangesAsync();
    }
}

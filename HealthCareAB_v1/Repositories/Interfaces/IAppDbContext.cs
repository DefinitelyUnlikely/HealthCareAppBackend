using System;
using HealthCareAB_v1.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Meeting> Meetings { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}


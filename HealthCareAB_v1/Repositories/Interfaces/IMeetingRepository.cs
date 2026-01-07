using System;
using HealthCareAB_v1.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IMeetingRepository
{
    public Task<Meeting> CreateAsync(Meeting meeting);
    public Task<int> SaveChangesAsync();
}

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAvailabilityRepository
{
    public Task SaveAvailabilityAsync(int userId, DateTime? from, DateTime? to);
    public Task GetAvailabilityAsync(int userId, DateTime? from, DateTime? to);
}
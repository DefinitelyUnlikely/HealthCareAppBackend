namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAvailabilityRepository
{
    public Task SaveAvailabilityAsync();
    public Task GetAvailabilityAsync();
}
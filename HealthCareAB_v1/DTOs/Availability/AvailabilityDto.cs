namespace HealthCareAB_v1.DTOs.Availability;

public class AvailabilityDto
{
    public List<int> CareGiverIds { get; set; } = [];
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
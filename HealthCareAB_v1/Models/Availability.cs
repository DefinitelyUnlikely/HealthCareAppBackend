namespace HealthCareAB_v1.Models;

public class Availability
{
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required int CaregiverId { get; set; }
}
   
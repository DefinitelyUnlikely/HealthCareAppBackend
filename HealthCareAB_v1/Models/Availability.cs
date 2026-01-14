namespace HealthCareAB_v1.Models;

public class Availability
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required int CaregiverId { get; set; }
}
   
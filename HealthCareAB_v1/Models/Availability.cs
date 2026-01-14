namespace HealthCareAB_v1.Models;

public class Availability
{
    public required Guid Id { get; set; } // We did specify this in the database diagram, but perhaps a bit overkill?
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required Caregiver Caregiver { get; set; }
}
   
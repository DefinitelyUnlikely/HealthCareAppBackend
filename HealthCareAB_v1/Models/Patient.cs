namespace HealthCareAB_v1.Models;

public class Patient : User
{
    public List<Caregiver> Caregivers { get; set; } = new List<Caregiver>();
}

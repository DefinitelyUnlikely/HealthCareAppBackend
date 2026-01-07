namespace HealthCareAB_v1.Models;

public class Caregiver : User
{
    public List<Qualification> Qualifications { get; set; } = new List<Qualification>();
    public List<Patient> Patients { get; set; } = new List<Patient>();
}
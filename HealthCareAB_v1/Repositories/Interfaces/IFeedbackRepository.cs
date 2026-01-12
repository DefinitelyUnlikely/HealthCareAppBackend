using HealthCareAB_v1.Models;
namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IFeedbackRepository
{
    Task<Feedback> CreateFeedbackAsync(Feedback feedback);
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(Guid id);
    Task<Feedback?> GetByMeetingIdAsync(Guid meetingId);
    Task<IEnumerable<Feedback>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Feedback>> GetByCaregiverIdAsync(int caregiverId);
    Task<Feedback> UpdateAsync(Feedback feedback);
    Task<bool> DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();

}
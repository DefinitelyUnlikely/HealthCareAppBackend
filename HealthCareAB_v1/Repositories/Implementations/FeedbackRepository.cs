using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HealthCareAB_v1.Repositories;


public class FeedbackRepository : IFeedbackRepository
{
    private readonly IAppDbContext _context;

    public FeedbackRepository(IAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
    {
        if (feedback == null)
            throw new ArgumentNullException(nameof(feedback));

        await _context.Feedbacks.AddAsync(feedback);
        await SaveChangesAsync();
        
        return feedback;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
       var feedback = await _context.Feedbacks.FindAsync(id);
        
        if (feedback == null)
            return false;

        _context.Feedbacks.Remove(feedback);
        await SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
         return await _context.Feedbacks
            .Include(f => f.Patient)
            .Include(f => f.Caregiver)
            .ToListAsync();
    }

    public async Task<IEnumerable<Feedback>> GetByCaregiverIdAsync(int caregiverId)
    {
        return await _context.Feedbacks
            .Include(f => f.Patient)
            .Where(f => f.CaregiverId == caregiverId)
            .ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(Guid id)
    {
         return await _context.Feedbacks
            .Include(f => f.Patient)
            .Include(f => f.Caregiver)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Feedback?> GetByMeetingIdAsync(Guid meetingId)
    {
        return await _context.Feedbacks
            .Include(f => f.Patient)
            .Include(f => f.Caregiver)
            .FirstOrDefaultAsync(f => f.MeetingId == meetingId);
    }

    public async Task<IEnumerable<Feedback>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Feedbacks
            .Include(f => f.Caregiver)
            .Where(f => f.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<Feedback> UpdateAsync(Feedback feedback)
    {
        if (feedback == null)
            throw new ArgumentNullException(nameof(feedback));

        var existingFeedback = await _context.Feedbacks.FindAsync(feedback.Id);
        
        if (existingFeedback == null)
            throw new KeyNotFoundException($"Feedback with ID {feedback.Id} not found");

        // Only review and rating can be updated
        existingFeedback.Review = feedback.Review;
        existingFeedback.Rating = feedback.Rating;

        _context.Feedbacks.Update(existingFeedback);
        await SaveChangesAsync();
        
        return existingFeedback;

    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

}
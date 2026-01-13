using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IUserService _userService;
    private readonly IMeetingRepository _meetingRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IUserService userService,
        IMeetingRepository meetingRepository)
    {
        _feedbackRepository = feedbackRepository;
        _userService = userService;
        _meetingRepository = meetingRepository;
    }
    public async Task<FeedbackResponseDto> CreateFeedbackAsync(CreateFeedbackDto dto, int currentUserId)
    {
        // Validates the user
        var user = await _userService.GetUserByIdAsync(currentUserId);
        if (user == null || !user.Roles.Contains("Patient"))
            throw new UnauthorizedAccessException("Only patients can create feedback");

        // call the meeting to get the caregiver
        var meeting = await _meetingRepository.GetAsync(dto.MeetingId);
        if (meeting == null)
            throw new ArgumentException("Meeting not found");

        if (meeting.PatientId != currentUserId)
            throw new UnauthorizedAccessException("You can only give feedback for your own meetings");

        // Create feeback
        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            Review = dto.Review,
            Rating = dto.Rating,
            MeetingId = dto.MeetingId,
            PatientId = currentUserId,
            CaregiverId = meeting.CaregiverId
        };

        var createdFeedback = await _feedbackRepository.CreateFeedbackAsync(feedback);
        return MapToResponseDto(createdFeedback);
    }

    public async Task<FeedbackResponseDto?> GetByIdAsync(Guid id)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(id);

        return feedback == null ? null : MapToResponseDto(feedback);
    }

    public async Task<IEnumerable<FeedbackResponseDto>> GetAllAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();

        return feedbacks.Select(MapToResponseDto);
    }

    public async Task<FeedbackResponseDto?> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto dto, int currentUserId)
    {
        var existingFeedback = await _feedbackRepository.GetByIdAsync(id);

        if (existingFeedback == null)
            return null;

        // Validates that the creater of the feedback must be the one to update it
        if (existingFeedback.PatientId != currentUserId)
            throw new UnauthorizedAccessException("You can only update your own feedback");

        // Updates the fields that were sent if they are not null
        if (dto.Review != null)
            existingFeedback.Review = dto.Review;

        if (dto.Rating.HasValue)
            existingFeedback.Rating = dto.Rating.Value;

        var updatedFeedback = await _feedbackRepository.UpdateAsync(existingFeedback);

        return MapToResponseDto(updatedFeedback);
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id, int currentUserId)
    {
        var existingFeedback = await _feedbackRepository.GetByIdAsync(id);

        if (existingFeedback == null)
            return false;

        // Only the creater of the feeback can delete it
        // or admin
        var user = await _userService.GetUserByIdAsync(currentUserId);
        if (existingFeedback.PatientId != currentUserId && !user.Roles.Contains("Admin"))
            throw new UnauthorizedAccessException("You can only delete your own feedback");

        return await _feedbackRepository.DeleteAsync(id);
    }

    // HELPER - Mapping
    private FeedbackResponseDto MapToResponseDto(Feedback feedback)
    {
        return new FeedbackResponseDto
        {
            Id = feedback.Id,
            Review = feedback.Review,
            Rating = feedback.Rating,
            PatientName = $"{feedback.Patient!.FirstName} {feedback.Patient.LastName}",
            CaregiverName = $"{feedback.Caregiver!.FirstName} {feedback.Caregiver.LastName}",
            MeetingId = feedback.MeetingId
        };
    }
}
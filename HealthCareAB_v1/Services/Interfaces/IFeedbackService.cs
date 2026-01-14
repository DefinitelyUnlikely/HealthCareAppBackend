using HealthCareAB_v1.DTOs;

public interface IFeedbackService
{
    Task<FeedbackResponseDto> CreateFeedbackAsync(CreateFeedbackDto dto, int currentUserId);
    Task<FeedbackResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<FeedbackResponseDto>> GetAllAsync();
    Task<FeedbackResponseDto?> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto dto, int currentUserId);
    Task<bool> DeleteFeedbackAsync(Guid id, int currentUserId);
}
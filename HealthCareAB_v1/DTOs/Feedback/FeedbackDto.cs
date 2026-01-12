using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs;

public class CreateFeedbackDto
{
    public string? Review { get; set; }
    [Range(1, 5)]
    public required int Rating { get; set; }
    public Guid MeetingId { get; set; }


}

public class UpdateFeedbackDto
{
    public string? Review { get; set; }  
    [Range(1, 5)]
    public int? Rating { get; set; }  
}

public class FeedbackResponseDto
{
    public required Guid Id { get; set; }
    public string? Review { get; set; }
    public required int Rating { get; set; }
    public required string PatientName { get; set; }
    public required string CaregiverName { get; set; }
    public required Guid MeetingId { get; set; }
    public DateTime CreatedAt { get; set; } 
}
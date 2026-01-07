namespace HealthCareAB_v1.Models.Notification;

public class Notification
{
    public required User ToUser { get; set; }
    public required string Message { get; set; }
}


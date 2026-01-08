namespace HealthCareAB_v1.Models.Notification;

public class Notification
{
    public virtual required User RecipientUser { get; set; }
    public virtual string Message { get; set; } = string.Empty;
}


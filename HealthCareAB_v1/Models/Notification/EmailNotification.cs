namespace HealthCareAB_v1.Models.Notification;

public class EmailNotification : Notification
{
    public required string Subject { get; set; }
    public required string Html { get; set; }
}

public class MeetingConfirmedEmailNotification : EmailNotification
{
    public required Meeting Meeting { get; set; }
}

public class MeetingCancelledEmailNotification : EmailNotification
{
    public required Meeting Meeting { get; set; }
}

public class MeetingReminderEmailNotification : EmailNotification
{
    public required Meeting Meeting { get; set; }
}

public class MeetingUpdatedEmailNotification : EmailNotification
{
    public required Meeting OldMeeting { get; set; }
    public required Meeting NewMeeting { get; set; }
}
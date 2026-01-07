namespace HealthCareAB_v1.Models.Notification;

public class EmailNotification : Notification
{
    public virtual required string Subject { get; set; }
    public virtual required string Html { get; set; }
}

public class MeetingConfirmedEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte bokat";
    public required Meeting Meeting { get; set; }

    public override required string Message { get; set; } = "Möte bokat";
    public override required string Html { get; set; } = "Möte bokat";
}

public class MeetingCancelledEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte avbokat";
    public required Meeting Meeting { get; set; }

    public override required string Message { get; set; } = "Möte avbokat";
    public override required string Html { get; set; } = "Möte avbokat";
}

public class MeetingReminderEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Mötespåminnelse";
    public required Meeting Meeting { get; set; }

    public override required string Message { get; set; } = "Mötespåminnelse";
    public override required string Html { get; set; } = "Mötespåminnelse";
}

public class MeetingUpdatedEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte uppdaterat";
    public required Meeting OldMeeting { get; set; }
    public required Meeting NewMeeting { get; set; }

    public override required string Message { get; set; } = "Möte uppdaterat";
    public override required string Html { get; set; } = "Möte uppdaterat";
}
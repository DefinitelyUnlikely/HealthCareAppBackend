namespace HealthCareAB_v1.Models.Notification;

public class EmailNotification : Notification
{
    public virtual required string Subject { get; set; }
    public virtual string Html { get; set; } = string.Empty;
}

public class MeetingConfirmedEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte bokat";
    public required Meeting Meeting { get; set; }

    public override string Message =>
        $"Hej, Möte bokat {Meeting.StartTime} på {Meeting.Caregiver?.Address}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte bokat {Meeting.StartTime} på {Meeting.Caregiver?.Address}</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingCancelledEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte avbokat";
    public required Meeting Meeting { get; set; }

    public override string Message =>
        $"Hej, Möte vid {Meeting.StartTime} på {Meeting.Caregiver?.Address} avbokat. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte vid {Meeting.StartTime} på {Meeting.Caregiver?.Address} avbokat</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingReminderEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Mötespåminnelse";
    public required Meeting Meeting { get; set; }

    public override string Message =>
        $"Hej, Mötespåminnelse för möte vid {Meeting.StartTime} på {Meeting.Caregiver?.Address}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Mötespåminnelse för möte vid {Meeting.StartTime} på {Meeting.Caregiver?.Address}</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingUpdatedEmailNotification : EmailNotification
{
    public override required string Subject { get; set; } = "Möte uppdaterat";
    public required Meeting OldMeeting { get; set; }
    public required Meeting NewMeeting { get; set; }

    public override string Message =>
    $"Hej, Möte uppdaterat {OldMeeting.StartTime} till {NewMeeting.StartTime} på {NewMeeting.Caregiver?.Address}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte uppdaterat {OldMeeting.StartTime} till {NewMeeting.StartTime} på {NewMeeting.Caregiver?.Address}</p><p>Hälsningar HealthCare AB</p>";
}

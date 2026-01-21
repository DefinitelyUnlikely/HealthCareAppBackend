namespace HealthCareAB_v1.Models.Notification;

public class EmailNotification : Notification
{
    public virtual string Subject { get; set; } = string.Empty;
    public virtual string Html { get; set; } = string.Empty;
}

public class MeetingConfirmedEmailNotification : EmailNotification
{
    public override string Subject { get; set; } = "Möte bokat";
    public required Meeting Meeting { get; set; }

    public string AddressMessage => Meeting.Caregiver != null ? " på " + Meeting.Caregiver.Address : string.Empty;

    public override string Message =>
        $"Hej, Möte bokat {Meeting.StartTime}{AddressMessage}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte bokat {Meeting.StartTime}{AddressMessage}</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingCancelledEmailNotification : EmailNotification
{
    public override string Subject { get; set; } = "Möte avbokat";
    public required Meeting Meeting { get; set; }

    public string AddressMessage => Meeting.Caregiver != null ? " på " + Meeting.Caregiver.Address : string.Empty;

    public override string Message =>
        $"Hej, Möte vid {Meeting.StartTime}{AddressMessage} avbokat. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte vid {Meeting.StartTime}{AddressMessage} avbokat</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingReminderEmailNotification : EmailNotification
{
    public override string Subject { get; set; } = "Mötespåminnelse";
    public required Meeting Meeting { get; set; }

    public string AddressMessage => Meeting.Caregiver != null ? " på " + Meeting.Caregiver.Address : string.Empty;

    public override string Message =>
        $"Hej, Mötespåminnelse för möte vid {Meeting.StartTime}{AddressMessage}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Mötespåminnelse för möte vid {Meeting.StartTime}{AddressMessage}</p><p>Hälsningar HealthCare AB</p>";
}

public class MeetingUpdatedEmailNotification : EmailNotification
{
    public override string Subject { get; set; } = "Möte uppdaterat";
    public required Meeting OldMeeting { get; set; }
    public required Meeting NewMeeting { get; set; }

    public string AddressMessage => NewMeeting.Caregiver != null ? " på " + NewMeeting.Caregiver.Address : string.Empty;

    public override string Message =>
        $"Hej, Möte uppdaterat från {OldMeeting.StartTime} till {NewMeeting.StartTime}{AddressMessage}. Hälsningar HealthCare AB";

    public override string Html =>
        $"<p>Hej,</p><p>Möte uppdaterat från {OldMeeting.StartTime} till {NewMeeting.StartTime}{AddressMessage}</p><p>Hälsningar HealthCare AB</p>";
}

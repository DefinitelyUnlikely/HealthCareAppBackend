using System.Runtime.InteropServices;

namespace HealthCareAB_v1.DTOs.Notification;

public abstract class Notification
{
    public string To { get; set; } = string.Empty;
    public virtual string Message { get; set; } = string.Empty;
}

public abstract class MeetingNotification : Notification
{
    public required DateTime MeetingTime { get; set; }
    public required string Location { get; set; }
}

public abstract class MeetingEmailNotification : MeetingNotification
{
    public required string Subject { get; set; }
    public virtual string HtmlMessage { get; set; } = string.Empty;
}

public class MeetingConfirmationEmailNotification : MeetingEmailNotification
{
    public override string Message =>
        $"Hej, ditt möte vid {Location} är bokat den {MeetingTime:yyyy-MM-dd HH:mm}.\n\nMed vänliga hälsningar, HealthCare AB";

    public override string HtmlMessage =>
        $"<p>Hej,</p>" +
        $"<p>Ditt möte vid <a href='http://maps.google.com/?q={Location}'>{Location}</a> är bokat den {MeetingTime:yyyy-MM-dd HH:mm}.</p>" +
        $"<p>Med vänliga hälsningar,<br>HealthCare AB</p>";
}

public class MeetingCancellationEmailNotification : MeetingEmailNotification
{
    public override string Message =>
        $"Hej, ditt möte vid {Location} den {MeetingTime:yyyy-MM-dd HH:mm} har blivit avbokat.\n\nMed vänliga hälsningar, HealthCare AB";

    public override string HtmlMessage =>
        $"<p>Hej,</p>" +
        $"<p>Ditt möte vid <a href='http://maps.google.com/?q={Location}'>{Location}</a> den {MeetingTime:yyyy-MM-dd HH:mm} har blivit avbokat.</p>" +
        $"<p>Med vänliga hälsningar,<br>HealthCare AB</p>";
}

public class MeetingRescheduledEmailNotification : MeetingEmailNotification
{
    public required DateTime OldMeetingTime { get; set; }

    public override string Message =>
        $"Hej, ditt möte vid {Location} den {OldMeetingTime:yyyy-MM-dd HH:mm} har blivit flyttat till {MeetingTime:yyyy-MM-dd HH:mm}.\n\nMed vänliga hälsningar, HealthCare AB";

    public override string HtmlMessage =>
        $"<p>Hej,</p>" +
        $"<p>Ditt möte vid <a href='http://maps.google.com/?q={Location}'>{Location}</a> den {OldMeetingTime:yyyy-MM-dd HH:mm} har blivit flyttat till {MeetingTime:yyyy-MM-dd HH:mm}.</p>" +
        $"<p>Med vänliga hälsningar,<br>HealthCare AB</p>";
}

public class MeetingReminderEmailNotification : MeetingEmailNotification
{
    public override string Message =>
        $"Hej, Glöm inte ditt möte vid {Location} den {MeetingTime:yyyy-MM-dd HH:mm}.\n\nMed vänliga hälsningar, HealthCare AB";

    public override string HtmlMessage =>
        $"<p>Hej,</p>" +
        $"<p>Glöm inte ditt möte vid <a href='http://maps.google.com/?q={Location}'>{Location}</a> den {MeetingTime:yyyy-MM-dd HH:mm}.</p>" +
        $"<p>Med vänliga hälsningar,<br>HealthCare AB</p>";
}

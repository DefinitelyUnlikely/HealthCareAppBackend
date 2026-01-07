public abstract class Notification
{
    public string To { get; set; }
    public string Message { get; set; }
}

public class ConfirmationNotification : Notification
{
}

public class CancelNotification : Notification
{
}

public class UpdateNotification : Notification
{
}


public class EmailConfirmationNotification : ConfirmationNotification
{
}

public class EmailCancelNotification : CancelNotification
{
}

public class EmailUpdateNotification : UpdateNotification
{
}

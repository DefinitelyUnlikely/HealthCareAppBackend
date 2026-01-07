namespace HealthCareAB_v1.Exceptions;

public class HandlerNotFoundException : Exception
{
    public HandlerNotFoundException()
    {
    }

    public HandlerNotFoundException(string message) : base(message)
    {
    }

    public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
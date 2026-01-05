namespace HealthCareAB_v1.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(Options options);

    // creating an options class to make it more flexible for future modifications
    public class Options
    {
        public required string To { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}

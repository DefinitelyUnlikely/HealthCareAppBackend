namespace HealthCareAB_v1.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(Options options);

    // if we want to add more options, we can create a class for it
    public class Options
    {
        public required string To { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}

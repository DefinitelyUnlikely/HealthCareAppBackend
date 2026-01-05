namespace HealthCareAB_v1.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(Email email);

    public class Email
    {
        public required string To { get; set; }
        public required string Subject { get; set; }
        public required string HtmlContent { get; set; }
    }
}

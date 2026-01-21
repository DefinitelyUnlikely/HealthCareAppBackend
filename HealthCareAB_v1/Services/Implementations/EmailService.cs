using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Utils;
using MimeKit;

namespace HealthCareAB_v1.Services.Implementations;

// In testing, we use Mailpit to mock the SMTP server. Production would use a real SMTP server. 
public class MimeKitEmailService(
    IConfiguration configuration,
    ISmtpClientFactory smtpClientFactory,
    ILogger<MimeKitEmailService> logger)
    : IEmailService
{
    private readonly string _smtpHost = configuration.GetSection("SMTP").GetValue<string>("Host") ?? "localhost";
    private readonly int _smtpPort = configuration.GetSection("SMTP").GetValue<int>("Port");

    public async Task SendEmailAsync(IEmailService.Email email)
    {
        if (!EmailValidator.IsValid(email.To))
        {
            throw new ArgumentException("Invalid email address.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("HealthCareAB", "health@care.ab"));
        message.To.Add(new MailboxAddress(email.To, email.To));
        message.Subject = email.Subject;


        var bodyBuilder = new BodyBuilder
        {
            TextBody = $"{email.PlainContent}"
        };

        if (email.HtmlContent is not null)
        {
            bodyBuilder.HtmlBody = $"{email.HtmlContent}";
        }

        message.Body = bodyBuilder.ToMessageBody();

        // This should probably prevent the email service to be a single point of failure.
        // If the email service fails, the application should continue to work.
        try
        {
            using var smtpClient = smtpClientFactory.CreateClient();
            await smtpClient.ConnectAsync(_smtpHost, _smtpPort);
            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {To}: {Message}", email.To, ex.Message);
        }
    }
}

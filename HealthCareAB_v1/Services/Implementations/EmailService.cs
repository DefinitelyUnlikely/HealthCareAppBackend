using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Utils;
using MimeKit;
using MimeKit.Text;

namespace HealthCareAB_v1.Services.Implementations;

public class MimeKitEmailService(IConfiguration configuration, ISmtpClientFactory smtpClientFactory)
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


        var bodyBuilder = new BodyBuilder();
        bodyBuilder.TextBody = $"{email.PlainContent}";

        if (email.HtmlContent is not null)
        {
            bodyBuilder.HtmlBody = $"{email.HtmlContent}";
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = smtpClientFactory.CreateClient();
        await smtpClient.ConnectAsync(_smtpHost, _smtpPort);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}

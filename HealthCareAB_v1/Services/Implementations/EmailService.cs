using HealthCareAB_v1.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace HealthCareAB_v1.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;

    public EmailService(IConfiguration configuration)
    {
        _smtpHost = configuration.GetSection("SMTP").GetValue<string>("Host") ?? "localhost";
        _smtpPort = configuration.GetSection("SMTP").GetValue<int>("Port");
    }

    public async Task SendEmailAsync(IEmailService.Email email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("HealthCareAB", "health@care.ab"));
        message.To.Add(new MailboxAddress(email.To, email.To));
        message.Subject = email.Subject;

        if (email.HtmlContent is not null)
        {
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"{email.HtmlContent}",
                TextBody = $"{email.PlainContent}"
            };
            message.Body = bodyBuilder.ToMessageBody();
        }
        else
        {
            message.Body = new TextPart(TextFormat.Plain) { Text = email.PlainContent };
        }

        using var smtpClient = new SmtpClient();
        // add server settings using env variables. MailPit can be used for local testing.
        await smtpClient.ConnectAsync(_smtpHost, _smtpPort);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
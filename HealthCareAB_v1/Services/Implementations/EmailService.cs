using HealthCareAB_v1.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace HealthCareAB_v1.Services.Implementations;

public class AzureEmailService : IEmailService
{
    public async Task SendEmailAsync(IEmailService.Email email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("HealthCareAB", "healthcareab@outlook.com"));
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
        await smtpClient.ConnectAsync(""); // add server settings using env variables.
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
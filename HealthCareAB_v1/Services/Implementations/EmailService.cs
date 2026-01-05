using HealthCareAB_v1.Services.Interfaces;
using MimeKit;

namespace HealthCareAB_v1.Services.Implementations;

public class AzureEmailService : IEmailService
{
    public Task SendEmailAsync(IEmailService.Email email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("HealthCareAB", "healthcareab@outlook.com"));
        message.To.Add(new MailboxAddress(email.To, email.To));
        message.Subject = email.Subject;
        message.Body = new TextPart("html") { Text = email.HtmlContent };

        return Task.CompletedTask;
    }
}
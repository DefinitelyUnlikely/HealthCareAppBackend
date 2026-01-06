using MimeKit;

namespace HealthCareAB_v1.Services.Interfaces;

// This is a wrapper for MailKits SMTP client - I need this to make proper unit tests for the EmailService.
// This interface is essentially just a copy of the stuff I need from MailKit's SMTP client. Takes the same arguments, 
// inherits the same interface etc. etc.
public interface ISmtpClient : IDisposable
{
    Task ConnectAsync(string host, int port = 0, bool useSsl = false, CancellationToken cancellationToken = default);
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
    Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);
}

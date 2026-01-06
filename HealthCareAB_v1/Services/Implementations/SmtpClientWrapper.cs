using MailKit.Net.Smtp;
using MimeKit;
using ISmtpClient = HealthCareAB_v1.Services.Interfaces.ISmtpClient;

namespace HealthCareAB_v1.Services.Implementations;

// This is a wrapper for MailKits SMTP client - I need this to make proper unit tests for the EmailService.
// But I don't really need any custom logic here, so it's essentially just a class that takes the same arguments 
// as the MailKit SMTP client and forwards them to that implementation...
public class SmtpClientWrapper : ISmtpClient
{
    private readonly SmtpClient _smtpClient;

    public SmtpClientWrapper()
    {
        _smtpClient = new SmtpClient();
    }

    public async Task ConnectAsync(string host, int port = 0, bool useSsl = false,
        CancellationToken cancellationToken = default)
    {
        await _smtpClient.ConnectAsync(host, port, useSsl, cancellationToken);
    }

    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        await _smtpClient.SendAsync(message, cancellationToken);
    }

    public async Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
    {
        await _smtpClient.DisconnectAsync(quit, cancellationToken);
    }

    public void Dispose()
    {
        _smtpClient.Dispose();
        // I don't quite understand this, but I get a warning telling me to include this line if I don't.
        GC.SuppressFinalize(this);
    }
}

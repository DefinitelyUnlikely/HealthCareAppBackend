using HealthCareAB_v1.Services.Interfaces;
using MailKit.Net.Smtp;

namespace HealthCareAB_v1.Services.Implementations;

public class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create()
    {
        return new SmtpClient();
    }
}
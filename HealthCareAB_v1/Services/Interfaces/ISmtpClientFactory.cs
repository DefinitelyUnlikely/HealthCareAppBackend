using MailKit.Net.Smtp;

namespace HealthCareAB_v1.Services.Interfaces;

public interface ISmtpClientFactory
{
    ISmtpClient Create();
}
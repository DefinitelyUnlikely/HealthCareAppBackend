using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class AzureEmailService : IEmailService
{
    public Task SendEmailAsync(IEmailService.Email email)
    {
        throw new NotImplementedException();
    }
}
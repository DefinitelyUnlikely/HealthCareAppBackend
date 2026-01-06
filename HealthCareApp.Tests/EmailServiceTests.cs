using Moq;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Implementations;
using Microsoft.Extensions.Configuration;

namespace HealthCareApp.Tests;

public class EmailServiceTests
{
    [Fact]
    public void SendEmailAsync_WhenCalled_SendsEmail()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string>
        {
            { "SMTP:Host", "localhost" },
            { "SMTP:Port", "1025" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var emailService = new EmailService(configuration);

        // Act


        // Assert
    }
}
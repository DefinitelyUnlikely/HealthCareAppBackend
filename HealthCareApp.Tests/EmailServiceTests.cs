using Moq;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Implementations;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace HealthCareApp.Tests;

public class EmailServiceTests
{
    [Fact]
    public async Task SendEmailAsync_WhenCalled_SendsEmail()
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

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act
        await emailService.SendEmailAsync(new IEmailService.Email
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlContent = "Test HTML Content",
            PlainContent = "Test Plain Content"
        });

        // Assert
        smptClientMock.Verify(client => client.ConnectAsync("localhost", 1025, false, It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendEmailAsync_MimeMessageContentIsCorrect()
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

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act
        await emailService.SendEmailAsync(new IEmailService.Email
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlContent = "Test HTML Content",
            PlainContent = "Test Plain Content"
        });

        // Assert
        smptClientMock.Verify(c => c.SendAsync(
                It.Is<MimeMessage>(m =>
                    m.Subject == "Test Subject" &&
                    m.To.Mailboxes.First().Address == "test@example.com" &&
                    m.From.Mailboxes.First().Address == "health@care.ab"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_SendsSuccessfully()
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

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act
        await emailService.SendEmailAsync(new IEmailService.Email
        {
            To = "test@example.com",
            Subject = "",
            HtmlContent = "Test HTML Content",
            PlainContent = "Test Plain Content"
        });

        // Assert
        smptClientMock.Verify(client => client.ConnectAsync("localhost", 1025, false, It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public void Constructor_WithMissingHost_UsesDefaultLocalhost()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string>
        {
            { "SMTP:Port", "1025" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act
        emailService.SendEmailAsync(new IEmailService.Email
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlContent = "Test HTML Content",
            PlainContent = "Test Plain Content"
        });

        // Assert
        smptClientMock.Verify(client => client.ConnectAsync("localhost", 1025, false, It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendEmailAsync_WithMissingPort_UsesDefaultPort()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string>
        {
            { "SMTP:Host", "localhost" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act
        emailService.SendEmailAsync(new IEmailService.Email
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlContent = "Test HTML Content",
            PlainContent = "Test Plain Content"
        });

        // Assert
        smptClientMock.Verify(client => client.ConnectAsync("localhost", 0, false, It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>()),
            Times.Once());
        smptClientMock.Verify(client => client.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task SendEmailAsync_WithInvalidEmail_ThrowsArgumentException()
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

        var smptClientMock = new Mock<ISmtpClient>();
        var smtpClientFactoryMock = new Mock<ISmtpClientFactory>();
        smtpClientFactoryMock.Setup(factory => factory.CreateClient()).Returns(smptClientMock.Object);

        var emailService = new MimeKitEmailService(configuration, smtpClientFactoryMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await emailService.SendEmailAsync(new IEmailService.Email
            {
                To = "invalid-email",
                Subject = "Test Subject",
                HtmlContent = "Test HTML Content",
                PlainContent = "Test Plain Content"
            });
        });
    }
}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using HealthCareAB_v1.Extensions;
using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Services.Implementations.Notifications;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Database and Application Services
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();

// Email and Notification
builder.Services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
builder.Services.AddScoped<IEmailService, MimeKitEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();

builder.Build().Run();

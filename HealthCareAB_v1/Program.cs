using HealthCareAB_v1.Extensions;
using HealthCareAB_v1.Models.Notification;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Services.Implementations.Notifications;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Services.Interfaces.Notifications;

var builder = WebApplication.CreateBuilder(args);

// === ALLT DETTA MÅSTE VARA FÖRE builder.Build() ===
builder.Services.AddControllers();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Important for cookies
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Move these to the service extension file?
builder.Services.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
builder.Services.AddScoped<IEmailService, MimeKitEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removed to work in dev with React Vite on http...
//app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

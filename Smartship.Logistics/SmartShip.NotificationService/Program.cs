/// <summary>
/// Provides backend implementation for Program.
/// </summary>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;
using SmartShip.EventBus.Infrastructure;
using SmartShip.NotificationService.BackgroundServices;
using SmartShip.NotificationService.Configurations;
using SmartShip.NotificationService.Integration;
using SmartShip.NotificationService.Services;
using SmartShip.Shared.Common.Handlers;
using SmartShip.Shared.Common.Logging;
using SmartShip.Shared.Common.Services;

// Load .env file for local development (values override appsettings.json via environment variables)
DotNetEnv.Env.Load();

var builder = Host.CreateApplicationBuilder(args);

// Ensure config is loaded from the service directory even when launched from another cwd.
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog
builder.Services.AddSerilog((_, loggerConfiguration) =>
    SmartShipSerilog.Configure(builder.Configuration, loggerConfiguration, "SmartShip.NotificationService"));

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.Configure<NotificationSettings>(builder.Configuration.GetSection(NotificationSettings.SectionName));
builder.Services.Configure<ServiceUrlsSettings>(builder.Configuration.GetSection(ServiceUrlsSettings.SectionName));

builder.Services.AddSingleton<RabbitMQConnectionManager>();
builder.Services.AddSingleton<IEventConsumer, RabbitMQConsumer>();

// ✓ Correlation ID Service Registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IIdentityContactClient, IdentityContactClient>((serviceProvider, client) =>
{
    var serviceUrls = serviceProvider.GetRequiredService<IOptions<ServiceUrlsSettings>>().Value;
    var baseUrl = serviceUrls.IdentityService?.Trim();

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = builder.Configuration["ServiceUrls:IdentityService"]?.Trim();
    }

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = builder.Environment.IsEnvironment("Docker")
            ? "http://identity-service:5001"
            : "http://localhost:5001";
    }

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("ServiceUrls:IdentityService is required for NotificationService.");
    }

    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddScoped<IEmailNotificationService, SmtpEmailNotificationService>();
builder.Services.AddHostedService<NotificationEventsConsumerService>();

var app = builder.Build();
await app.RunAsync();



/// <summary>
/// Application entry point and service composition root for the Admin microservice.
/// Configures authentication, database access, message bus consumers, and the HTTP pipeline.
/// </summary>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartShip.AdminService.BackgroundServices;
using SmartShip.AdminService.Data;
using SmartShip.AdminService.Middleware;
using SmartShip.AdminService.Repositories;
using SmartShip.AdminService.Services;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;
using SmartShip.EventBus.Infrastructure;
using SmartShip.Shared.Common.Configuration;
using SmartShip.Shared.Common.Logging;
using SmartShip.Shared.Common.Handlers;
using SmartShip.Shared.Common.Middleware;
using SmartShip.Shared.Common.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartShip.AdminService;

/// <summary>
/// Bootstrap class for the Admin microservice.
/// Handles dependency injection, middleware pipeline, and application startup.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point. Configures and launches the Admin API server.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        // Load .env file for local development (values override appsettings.json via environment variables)
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        #region Logging Configuration

        // Configure Serilog as the structured logging provider
        builder.Host.UseSerilog((context, loggerConfiguration) =>
            SmartShipSerilog.Configure(context.Configuration, loggerConfiguration, "SmartShip.AdminService"));

        #endregion

        #region Core Service Registration

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();

        #endregion

        #region Swagger / OpenAPI Configuration

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartShip Admin API",
                Version = "v1"
            });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        #endregion

        #region Authentication & Authorization

        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings are missing in configuration.");
        jwtSettings.Validate();
        var validAudiences = jwtSettings.GetValidAudiences();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudiences = validAudiences,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        builder.Services.AddAuthorization();

        #endregion

        #region Infrastructure & Data Access

        // Correlation ID for distributed tracing
        builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

        // Database context registration
        builder.Services.AddDbContext<AdminDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SmartShipAdminServiceConnection")
                ?? "Server=.;Database=SmartShipAdminDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"));

        // RabbitMQ event bus configuration
        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
        builder.Services.AddSingleton<RabbitMQConnectionManager>();
        builder.Services.AddSingleton<IEventConsumer, RabbitMQConsumer>();

        #endregion

        #region Inter-Service HTTP Communication

        var shipmentServiceUrl = builder.Configuration["ServiceUrls:ShipmentService"]
            ?? throw new InvalidOperationException("ServiceUrls:ShipmentService is required.");

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
        builder.Services.AddHttpClient<global::SmartShip.AdminService.Integration.IShipmentClient, global::SmartShip.AdminService.Integration.ShipmentClient>(client =>
        {
            client.BaseAddress = new Uri(shipmentServiceUrl);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        #endregion

        #region Application Services & Repositories

        builder.Services.AddScoped<IAdminRepository, AdminRepository>();
        builder.Services.AddScoped<IAdminService, global::SmartShip.AdminService.Services.AdminService>();

        #endregion

        #region Background Services

        builder.Services.AddHostedService<AdminShipmentExceptionConsumerService>();
        builder.Services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        #endregion

        var app = builder.Build();

        #region HTTP Pipeline Configuration

        // Structured request logging via Serilog
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                diagnosticContext.Set("UserId", httpContext.User?.FindFirst("sub")?.Value ?? "anonymous");
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            };
        });

        // Correlation ID middleware — must be early in pipeline
        app.UseCorrelationId();

        // Global exception handler
        app.UseGlobalExceptionHandling();

        // Swagger UI (development & Docker environments only)
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // HTTPS redirection (skip in Docker to allow reverse proxy handling)
        if (!app.Environment.IsEnvironment("Docker"))
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        #endregion

        app.Run();
    }
}

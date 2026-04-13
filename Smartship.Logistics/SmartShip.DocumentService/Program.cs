/// <summary>
/// Provides backend implementation for Program.
/// </summary>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartShip.DocumentService.BackgroundServices;
using SmartShip.DocumentService.Data;
using SmartShip.DocumentService.Middleware;
using SmartShip.DocumentService.Repositories;
using SmartShip.DocumentService.Services;
using SmartShip.DocumentService.Storage;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;
using SmartShip.EventBus.Infrastructure;
using SmartShip.Shared.Common.Configuration;
using SmartShip.Shared.Common.Logging;
using SmartShip.Shared.Common.Middleware;
using SmartShip.Shared.Common.Services;
using System.Text;

namespace SmartShip.DocumentService;

/// <summary>
/// Represents Program.
/// </summary>
public class Program
{
    /// <summary>
    /// Executes the Main operation.
    /// </summary>
    public static void Main(string[] args)
    {
        // Load .env file for local development (values override appsettings.json via environment variables)
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, loggerConfiguration) =>
            SmartShipSerilog.Configure(context.Configuration, loggerConfiguration, "SmartShip.DocumentService"));

        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartShip Document API",
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

        // ✓ Correlation ID Service Registration
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

        builder.Services.AddDbContext<DocumentDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SmartShipDocumentServiceConnection")
                ?? "Server=.;Database=SmartShipDocumentDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"));

        builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
        builder.Services.AddSingleton<RabbitMQConnectionManager>();
        builder.Services.AddSingleton<IEventConsumer, RabbitMQConsumer>();

        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
        builder.Services.AddScoped<IDocumentService, global::SmartShip.DocumentService.Services.DocumentService>();

        builder.Services.AddHostedService<DocumentShipmentEventsConsumerService>();
        builder.Services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        var app = builder.Build();

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

        // ✓ Correlation ID Middleware - Must be early in pipeline
        app.UseCorrelationId();

        app.UseStaticFiles();
        app.UseGlobalExceptionHandling();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!app.Environment.IsEnvironment("Docker"))
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}



/// <summary>
/// Provides backend implementation for Program.
/// </summary>

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;
using SmartShip.EventBus.Infrastructure;
using SmartShip.IdentityService.Configurations;
using SmartShip.IdentityService.Data;
using SmartShip.IdentityService.Middleware;
using SmartShip.IdentityService.Repositories;
using SmartShip.IdentityService.Security;
using SmartShip.IdentityService.Services;
using SmartShip.IdentityService.Swagger;
using SmartShip.Shared.Common.Configuration;
using SmartShip.Shared.Common.Logging;
using SmartShip.Shared.Common.Middleware;
using SmartShip.Shared.Common.Services;
using System.Text;
using System.Threading.RateLimiting;

namespace SmartShip.IdentityService
{
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
                SmartShipSerilog.Configure(context.Configuration, loggerConfiguration, "SmartShip.IdentityService"));

            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services.AddMemoryCache();

            builder.Services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SmartShipIdentityServiceConnection")));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartShip Identity API",
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

                options.OperationFilter<AuthorizeOperationFilter>();
            });

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings are missing in configuration.");
            jwtSettings.Validate(requireExpiryMinutes: true);
            var validAudiences = jwtSettings.GetValidAudiences();

            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddScoped<JwtTokenGenerator>();

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            {
                KeyId = "smartship-jwt-signing-key"
            };

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
                        IssuerSigningKey = signingKey,
                        IssuerSigningKeyResolver = (_, _, _, _) => new[] { signingKey }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("AuthSensitive", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            // ✓ Correlation ID Service Registration
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));
            builder.Services.Configure<InternalServiceAuthSettings>(builder.Configuration.GetSection(InternalServiceAuthSettings.SectionName));
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
            builder.Services.AddSingleton<RabbitMQConnectionManager>();
            builder.Services.AddScoped<IEventPublisher, RabbitMQPublisher>();

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

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (!app.Environment.IsEnvironment("Docker"))
            {
                app.UseHttpsRedirection();
            }
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}



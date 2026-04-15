using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Serilog;
using SmartShip.Gateway.DelegatingHandlers;
using SmartShip.Shared.Common.Handlers;
using SmartShip.Shared.Common.Logging;
using SmartShip.Shared.Common.Middleware;
using SmartShip.Shared.Common.Services;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;

namespace SmartShip.Gateway;

/// <summary>
/// Application entry point that configures services and starts the host.
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point that configures services and starts the host.
    /// </summary>
    public static async Task Main(string[] args)
    {
        // Load .env file for local development (values override appsettings.json via environment variables)
        DotNetEnv.Env.Load();

        var builder = WebApplication.CreateBuilder(args);
        var environmentName = builder.Environment.EnvironmentName;

        builder.Configuration
            .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"ocelot.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"ocelot.{environmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("ocelot.SwaggerEndPoints.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"ocelot.SwaggerEndPoints.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"ocelot.SwaggerEndPoints.{environmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Host.UseSerilog((context, loggerConfiguration) =>
            SmartShipSerilog.Configure(context.Configuration, loggerConfiguration, "SmartShip.Gateway"));

        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();

        var jwtSection = builder.Configuration.GetSection("JwtSettings");
        var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is missing.");
        var jwtIssuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is missing.");
        var jwtAudiences = jwtSection.GetSection("Audiences").Get<string[]>()
            ?.Where(audience => !string.IsNullOrWhiteSpace(audience))
            .Select(audience => audience.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (jwtAudiences is null || jwtAudiences.Length == 0)
        {
            throw new InvalidOperationException("JwtSettings:Audiences is missing.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "GatewayJwt";
                options.DefaultChallengeScheme = "GatewayJwt";
            })
            .AddJwtBearer("GatewayJwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudiences = jwtAudiences,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser().RequireRole("Admin"));

            options.AddPolicy("TrackingRead", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("permissions", "tracking.read"));
        });

        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("GatewayCors", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // ✓ Correlation ID Service Registration
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddTransient<DownstreamTelemetryHandler>();
        builder.Services.AddTransient<CorrelationIdDelegatingHandler>();
        builder.Services
            .AddOcelot(builder.Configuration)
            .AddDelegatingHandler<DownstreamTelemetryHandler>(global: true)
            .AddDelegatingHandler<CorrelationIdDelegatingHandler>(global: true)
            .AddPolly()
            .AddCacheManager(settings => settings.WithDictionaryHandle())
            .AddConsul();

        var app = builder.Build();

        var swaggerSourceUrls = builder.Configuration
            .GetSection("SwaggerEndPoints")
            .GetChildren()
            .Select(endpointSection => new
            {
                Key = endpointSection["Key"],
                Url = endpointSection
                    .GetSection("Config")
                    .GetChildren()
                    .FirstOrDefault()?["Url"]
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Url))
            .ToDictionary(
                x => x.Key!.Trim().ToLowerInvariant(),
                x => x.Url!.Trim(),
                StringComparer.OrdinalIgnoreCase);

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

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "Unhandled exception in gateway pipeline.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Gateway processing error",
                    Detail = "An unexpected error occurred while processing the request."
                });
            });
        });

        app.UseStatusCodePages(async statusContext =>
        {
            var response = statusContext.HttpContext.Response;

            if (response.HasStarted || response.ContentLength > 0)
            {
                return;
            }

            if (response.StatusCode is not (StatusCodes.Status502BadGateway or StatusCodes.Status503ServiceUnavailable or StatusCodes.Status504GatewayTimeout or StatusCodes.Status429TooManyRequests))
            {
                return;
            }

            await response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = response.StatusCode,
                Title = response.StatusCode switch
                {
                    StatusCodes.Status429TooManyRequests => "Too many requests",
                    StatusCodes.Status503ServiceUnavailable => "Service unavailable",
                    StatusCodes.Status504GatewayTimeout => "Gateway timeout",
                    _ => "Downstream service unavailable"
                },
                Detail = "The gateway could not complete your request due to downstream limitations."
            });
        });

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Docker"))
        {
            app.UseHttpsRedirection();
        }
        app.UseCors("GatewayCors");
        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.Equals("/", StringComparison.OrdinalIgnoreCase) ||
                            context.Request.Path.Equals("/swagger", StringComparison.OrdinalIgnoreCase) ||
                            context.Request.Path.Equals("/swagger/index.html", StringComparison.OrdinalIgnoreCase))
            {
                const string html = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SmartShip Gateway APIs</title>
    <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
</head>
<body>
    <div id="swagger-ui"></div>
    <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
    <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-standalone-preset.js"></script>
    <script>
        window.ui = SwaggerUIBundle({
            urls: [
                { url: '/swagger/gatewaydocs/identity', name: 'Identity Service' },
                { url: '/swagger/gatewaydocs/shipment', name: 'Shipment Service' },
                { url: '/swagger/gatewaydocs/tracking', name: 'Tracking Service' },
                { url: '/swagger/gatewaydocs/document', name: 'Document Service' },
                { url: '/swagger/gatewaydocs/admin', name: 'Admin Service' }
            ],
            dom_id: '#swagger-ui',
            deepLinking: true,
            persistAuthorization: true,
            presets: [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset],
            layout: 'StandaloneLayout'
        });
    </script>
</body>
</html>
""";

                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/swagger/gatewaydocs", out var remainingPath))
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var correlationId = context.RequestServices.GetRequiredService<ICorrelationIdService>().GetCorrelationId();
                var serviceKey = remainingPath.Value?.Trim('/').ToLowerInvariant() ?? string.Empty;
                var routePrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["identity"] = "/identity",
                    ["shipment"] = "/shipment",
                    ["tracking"] = "/tracking",
                    ["document"] = "/document",
                    ["admin"] = "/admin"
                };

                if (!routePrefixes.TryGetValue(serviceKey, out var routePrefix))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Unknown swagger service key.");
                    return;
                }

                using var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(15)
                };
                if (!string.IsNullOrWhiteSpace(correlationId) && !client.DefaultRequestHeaders.Contains("X-Correlation-ID"))
                {
                    client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
                }
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SmartShip-Gateway-SwaggerDocs/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                var sourceUrl = swaggerSourceUrls.TryGetValue(serviceKey, out var configuredSwaggerUrl)
                    ? configuredSwaggerUrl
                    : $"{context.Request.Scheme}://{context.Request.Host}/{serviceKey}/swagger/v1/swagger.json";

                logger.LogDebug("Fetching swagger document for {ServiceKey} from {SourceUrl}", serviceKey, sourceUrl);
                string sourceJson;

                try
                {
                    sourceJson = await client.GetStringAsync(sourceUrl);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Unable to fetch swagger document for service {ServiceKey} from {SourceUrl}", serviceKey, sourceUrl);
                    context.Response.StatusCode = StatusCodes.Status502BadGateway;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status502BadGateway,
                        Title = "Swagger source unavailable",
                        Detail = $"Failed to retrieve swagger for '{serviceKey}' from '{sourceUrl}'. {ex.Message}"
                    });
                    return;
                }

                JsonNode? rootNode;
                try
                {
                    rootNode = JsonNode.Parse(sourceJson);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Invalid swagger document",
                        Detail = ex.Message
                    });
                    return;
                }

                if (rootNode is not JsonObject rootObject)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Invalid swagger object.");
                    return;
                }

                if (rootObject["paths"] is JsonObject pathsObject)
                {
                    var rewrittenPaths = new JsonObject();
                    foreach (var pathEntry in pathsObject)
                    {
                        if (pathEntry.Key is null || pathEntry.Value is null)
                        {
                            continue;
                        }

                        var normalizedPath = pathEntry.Key.StartsWith('/')
                                    ? pathEntry.Key
                                    : "/" + pathEntry.Key;

                        var prefixedPath = routePrefix + normalizedPath;
                        rewrittenPaths[prefixedPath] = pathEntry.Value.DeepClone();
                    }

                    rootObject["paths"] = rewrittenPaths;
                }

                rootObject["servers"] = new JsonArray
                        {
                                    new JsonObject
                                    {
                                        ["url"] = $"{context.Request.Scheme}://{context.Request.Host}"
                                    }
                        };

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(rootObject.ToJsonString(new JsonSerializerOptions
                {
                    WriteIndented = false
                }));
                return;
            }

            await next();
        });

        app.MapHealthChecks("/health").AllowAnonymous();

        await app.UseOcelot();
        await app.RunAsync();
    }
}



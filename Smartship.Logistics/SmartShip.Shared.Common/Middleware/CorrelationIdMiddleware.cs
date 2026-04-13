/// <summary>
/// Middleware for handling correlation ID across requests.
/// Ensures every request has a correlation ID extracted from headers or generated new.
/// </summary>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Services;

namespace SmartShip.Shared.Common.Middleware;

/// <summary>
/// Middleware for correlation ID management.
/// Extracts X-Correlation-ID from request headers or generates a new one.
/// Sets it in response headers and HttpContext for downstream access.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>
    /// Initializes a new instance of the correlation id middleware class.
    /// </summary>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously handles the invoke async process.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ExtractCorrelationId(context);
        context.Items[CorrelationIdContextKey] = correlationId;
        context.TraceIdentifier = correlationId;
        CorrelationIdAccessor.SetCorrelationId(correlationId);

        if (!context.Response.HasStarted)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                    context.Response.Headers[CorrelationIdHeaderName] = correlationId;
                return Task.CompletedTask;
            });
        }

        try
        {
            await _next(context);
        }
        finally
        {
            CorrelationIdAccessor.Clear();
        }
    }

    /// <summary>
    /// Extracts correlation ID from request headers or generates a new one.
    /// </summary>
    private string ExtractCorrelationId(HttpContext context)
    {
        // ✓ Try to get from X-Correlation-ID header (from Gateway/upstream service)
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
        {
            var correlationId = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.Trim();
            }
        }

        // ✓ Try to get from request trace identifier (set by TraceIdMiddleware or others)
        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        // ✓ Generate new correlation ID if not present
        var newCorrelationId = Guid.NewGuid().ToString("D");
        _logger.LogDebug("Generated new CorrelationId: {CorrelationId}", newCorrelationId);
        return newCorrelationId;
    }
}

/// <summary>
/// Extension methods for CorrelationIdMiddleware registration.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Adds CorrelationIdMiddleware to the request pipeline.
    /// Should be called early in the pipeline, often as the first middleware.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}

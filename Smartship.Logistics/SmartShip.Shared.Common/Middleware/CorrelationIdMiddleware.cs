using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Services;

namespace SmartShip.Shared.Common.Middleware;

/// <summary>
/// Middleware component for correlation id request pipeline behavior.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>
    /// Processes correlation id middleware behavior in the request pipeline.
    /// </summary>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes invoke asynchronously.
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
    /// Processes extract correlation id.
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
/// Domain model for correlation id middleware extensions.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Registers correlation id.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}

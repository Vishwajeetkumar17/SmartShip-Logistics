using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartShip.Shared.Common.Services;

/// <summary>
/// Defines correlation id business operations used by the service layer.
/// </summary>
    public interface ICorrelationIdService
{
    string GetCorrelationId();
    void SetCorrelationId(string correlationId);
}

/// <summary>
/// Implements correlation id business workflows for SmartShip logistics operations.
/// </summary>
public sealed class CorrelationIdService : ICorrelationIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CorrelationIdService> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>
    /// Implements correlation id service workflows.
    /// </summary>
    public CorrelationIdService(IHttpContextAccessor httpContextAccessor, ILogger<CorrelationIdService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Returns correlation id.
    /// </summary>
    public string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Priority 1: HttpContext.Items (set by middleware)
        if (httpContext?.Items.TryGetValue(CorrelationIdContextKey, out var contextValue) == true)
        {
            if (contextValue is string id && !string.IsNullOrWhiteSpace(id))
                return id;
        }

        // Priority 2: X-Correlation-ID header
        if (httpContext?.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue) == true)
        {
            var id = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(id))
                return id.Trim();
        }

        // Priority 3: TraceIdentifier
        if (!string.IsNullOrWhiteSpace(httpContext?.TraceIdentifier))
            return httpContext!.TraceIdentifier;

        // Priority 4: AsyncLocal (background tasks)
        var asyncId = CorrelationIdAccessor.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(asyncId))
            return asyncId;

        // Priority 5: Generate new
        var newId = Guid.NewGuid().ToString("D");
        CorrelationIdAccessor.SetCorrelationId(newId);
        return newId;
    }

    /// <summary>
    /// Sets correlation id.
    /// </summary>
    public void SetCorrelationId(string correlationId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && !httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers[CorrelationIdHeaderName] = correlationId;
        }
    }
}


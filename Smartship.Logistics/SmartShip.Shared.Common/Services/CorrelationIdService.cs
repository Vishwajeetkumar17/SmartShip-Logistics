/// <summary>
/// Provides correlation ID management for request tracing across services.
/// Retrieves correlation ID from HttpContext items or headers for distributed tracing.
/// </summary>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartShip.Shared.Common.Services;

/// <summary>
    /// Defines the contract for the correlation id service structure.
    /// </summary>
    public interface ICorrelationIdService
{
    string GetCorrelationId();
    void SetCorrelationId(string correlationId);
}

/// <summary>
/// Provides default implementation of ICorrelationIdService.
/// Production-level implementation with proper error handling and fallback mechanisms.
/// </summary>
public sealed class CorrelationIdService : ICorrelationIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CorrelationIdService> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>
    /// Initializes a new instance of the correlation id service class.
    /// </summary>
    public CorrelationIdService(IHttpContextAccessor httpContextAccessor, ILogger<CorrelationIdService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asynchronously handles the get correlation id process.
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
    /// Asynchronously handles the set correlation id process.
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


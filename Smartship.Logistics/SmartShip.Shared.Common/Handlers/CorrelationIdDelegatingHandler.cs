using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Services;

namespace SmartShip.Shared.Common.Handlers;

/// <summary>
/// Handler component for correlation id delegating processing.
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<CorrelationIdDelegatingHandler> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    /// <summary>
    /// Processes correlation id delegating handler.
    /// </summary>
    public CorrelationIdDelegatingHandler(
        ICorrelationIdService correlationIdService,
        ILogger<CorrelationIdDelegatingHandler> logger)
    {
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdService.GetCorrelationId();
        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(CorrelationIdHeaderName))
        {
            request.Headers.Add(CorrelationIdHeaderName, correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

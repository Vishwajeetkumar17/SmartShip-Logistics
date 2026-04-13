/// <summary>
/// Delegating handler that automatically adds correlation ID to outgoing HTTP requests.
/// Ensures every service-to-service call includes the correlation ID for distributed tracing.
/// </summary>

using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Services;

namespace SmartShip.Shared.Common.Handlers;

/// <summary>
/// HttpClient delegating handler that propagates correlation ID to downstream services.
/// Add to HttpClientBuilder: .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly ICorrelationIdService _correlationIdService;
    private readonly ILogger<CorrelationIdDelegatingHandler> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

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

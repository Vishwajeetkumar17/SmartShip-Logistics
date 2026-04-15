namespace SmartShip.Gateway.DelegatingHandlers;

/// <summary>
/// Handler component for downstream telemetry processing.
/// </summary>
public class DownstreamTelemetryHandler(ILogger<DownstreamTelemetryHandler> logger) : DelegatingHandler
{
    #region Protected API
    /// <summary>
    /// Sends async.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;

            logger.LogInformation(
                "Downstream call {Method} {Uri} responded {StatusCode} in {ElapsedMs}ms",
                request.Method.Method,
                request.RequestUri,
                (int)response.StatusCode,
                elapsedMs);

            return response;
        }
        catch (Exception ex)
        {
            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;

            logger.LogError(
                ex,
                "Downstream call failed {Method} {Uri} after {ElapsedMs}ms",
                request.Method.Method,
                request.RequestUri,
                elapsedMs);

            throw;
        }
    }
    #endregion
}



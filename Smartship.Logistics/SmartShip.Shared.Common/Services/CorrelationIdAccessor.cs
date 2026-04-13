/// <summary>
/// Provides AsyncLocal storage for correlation ID across async request flows.
/// Allows correlation ID access even when HttpContext may not be available.
/// </summary>

namespace SmartShip.Shared.Common.Services;

/// <summary>
/// Manages correlation ID using AsyncLocal for reliable storage across async/await chains.
/// Essential for background tasks, Timer callbacks, and async operations that don't have HttpContext.
/// </summary>
public sealed class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Gets the correlation ID from AsyncLocal storage.
    /// Returns null if not set in current async context.
    /// </summary>
    public static string? GetCorrelationId() => _correlationId.Value;

    /// <summary>
    /// Sets the correlation ID in AsyncLocal storage.
    /// Automatically propagates to child async tasks.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    public static void SetCorrelationId(string? correlationId) => _correlationId.Value = correlationId;

    /// <summary>
    /// Clears the correlation ID from AsyncLocal storage.
    /// </summary>
    public static void Clear() => _correlationId.Value = null;
}

namespace SmartShip.Shared.Common.Services;

/// <summary>
/// Domain model for correlation id accessor.
/// </summary>
public sealed class CorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// Returns correlation id.
    /// </summary>
    public static string? GetCorrelationId() => _correlationId.Value;

    /// <summary>
    /// Sets correlation id.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set.</param>
    public static void SetCorrelationId(string? correlationId) => _correlationId.Value = correlationId;

    /// <summary>
    /// Processes clear.
    /// </summary>
    public static void Clear() => _correlationId.Value = null;
}

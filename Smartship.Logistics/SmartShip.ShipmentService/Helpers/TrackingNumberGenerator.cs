namespace SmartShip.ShipmentService.Helpers;

using SmartShip.Shared.Common.Helpers;

/// <summary>
/// Domain model for tracking number generator.
/// </summary>
public static class TrackingNumberGenerator
{
    /// <summary>
    /// Generates tracking number.
    /// </summary>
    public static string GenerateTrackingNumber() => $"SS-{TimeZoneHelper.GetCurrentIstTime():yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}



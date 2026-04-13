/// <summary>
/// Provides backend implementation for TrackingNumberGenerator.
/// </summary>

namespace SmartShip.ShipmentService.Helpers;

using SmartShip.Shared.Common.Helpers;

/// <summary>
/// Represents TrackingNumberGenerator.
/// </summary>
public static class TrackingNumberGenerator
{
    /// <summary>
    /// Executes GenerateTrackingNumber.
    /// </summary>
    public static string GenerateTrackingNumber() => $"SS-{TimeZoneHelper.GetCurrentIstTime():yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}



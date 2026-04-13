/// <summary>
/// Provides backend implementation for DelayExceptionMonitoringOptions.
/// </summary>

namespace SmartShip.ShipmentService.Configuration;

/// <summary>
/// Represents DelayExceptionMonitoringOptions.
/// </summary>
public sealed class DelayExceptionMonitoringOptions
{
    public const string SectionName = "DelayExceptionMonitoring";

    /// <summary>
    /// Gets or sets the check interval minutes.
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 10;
    /// <summary>
    /// Gets or sets the standard threshold hours.
    /// </summary>
    public int StandardThresholdHours { get; set; } = 120;
    /// <summary>
    /// Gets or sets the express threshold hours.
    /// </summary>
    public int ExpressThresholdHours { get; set; } = 48;
    /// <summary>
    /// Gets or sets the economy threshold hours.
    /// </summary>
    public int EconomyThresholdHours { get; set; } = 168;
}



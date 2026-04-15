namespace SmartShip.ShipmentService.Configuration;

/// <summary>
/// Configuration model for delay exception monitoring settings.
/// </summary>
public sealed class DelayExceptionMonitoringOptions
{
    public const string SectionName = "DelayExceptionMonitoring";
    public int CheckIntervalMinutes { get; set; } = 10;
    public int StandardThresholdHours { get; set; } = 120;
    public int ExpressThresholdHours { get; set; } = 48;
    public int EconomyThresholdHours { get; set; } = 168;
}



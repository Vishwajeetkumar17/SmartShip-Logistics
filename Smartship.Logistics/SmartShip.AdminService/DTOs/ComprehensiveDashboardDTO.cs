/// <summary>
/// Provides backend implementation for ComprehensiveDashboardDTO.
/// </summary>

using System.Collections.Generic;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents ComprehensiveDashboardDTO.
/// </summary>
public class ComprehensiveDashboardDTO
{
    public StatMetric TotalShipments { get; set; } = new();
    public StatMetric InTransit { get; set; } = new();
    public StatMetric Delivered { get; set; } = new();
    public StatMetric Exceptions { get; set; } = new();
    public double OnTimeDeliveryPercent { get; set; }
    public double AvgDeliveryTimeDays { get; set; }
    public List<DailyShipment> DailyShipments { get; set; } = new();
    public List<ServiceDistribution> ServiceTypeDistribution { get; set; } = new();
    public List<PerformanceTrend> DeliveryPerformanceTrend { get; set; } = new();
}

/// <summary>
/// Represents StatMetric.
/// </summary>
public class StatMetric
{
    public int Count { get; set; }
    public int PercentageChange { get; set; }
}

/// <summary>
/// Represents DailyShipment.
/// </summary>
public class DailyShipment
{
    public string Date { get; set; } = string.Empty;
    public int Shipments { get; set; }
}

/// <summary>
/// Represents ServiceDistribution.
/// </summary>
public class ServiceDistribution
{
    public string ServiceType { get; set; } = string.Empty;
    public double Percentage { get; set; }
}

/// <summary>
/// Represents PerformanceTrend.
/// </summary>
public class PerformanceTrend
{
    public string Month { get; set; } = string.Empty;
    public double OnTimePercent { get; set; }
    public double DelayedPercent { get; set; }
}



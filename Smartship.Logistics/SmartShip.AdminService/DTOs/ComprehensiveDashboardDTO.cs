using System.Collections.Generic;

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Data transfer model for comprehensive dashboard payloads.
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
/// Domain model for stat metric.
/// </summary>
public class StatMetric
{
    public int Count { get; set; }
    public int PercentageChange { get; set; }
}

/// <summary>
/// Domain model for daily shipment.
/// </summary>
public class DailyShipment
{
    public string Date { get; set; } = string.Empty;
    public int Shipments { get; set; }
}

/// <summary>
/// Domain model for service distribution.
/// </summary>
public class ServiceDistribution
{
    public string ServiceType { get; set; } = string.Empty;
    public double Percentage { get; set; }
}

/// <summary>
/// Domain model for performance trend.
/// </summary>
public class PerformanceTrend
{
    public string Month { get; set; } = string.Empty;
    public double OnTimePercent { get; set; }
    public double DelayedPercent { get; set; }
}



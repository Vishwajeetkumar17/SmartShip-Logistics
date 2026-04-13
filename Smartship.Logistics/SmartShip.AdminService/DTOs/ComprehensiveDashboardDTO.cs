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
    /// <summary>
    /// Gets or sets the total shipments.
    /// </summary>
    public StatMetric TotalShipments { get; set; } = new();
    /// <summary>
    /// Gets or sets the in transit.
    /// </summary>
    public StatMetric InTransit { get; set; } = new();
    /// <summary>
    /// Gets or sets the delivered.
    /// </summary>
    public StatMetric Delivered { get; set; } = new();
    /// <summary>
    /// Gets or sets the exceptions.
    /// </summary>
    public StatMetric Exceptions { get; set; } = new();
    /// <summary>
    /// Gets or sets the on time delivery percent.
    /// </summary>
    public double OnTimeDeliveryPercent { get; set; }
    /// <summary>
    /// Gets or sets the avg delivery time days.
    /// </summary>
    public double AvgDeliveryTimeDays { get; set; }
    /// <summary>
    /// Gets or sets the daily shipments.
    /// </summary>
    public List<DailyShipment> DailyShipments { get; set; } = new();
    /// <summary>
    /// Gets or sets the service type distribution.
    /// </summary>
    public List<ServiceDistribution> ServiceTypeDistribution { get; set; } = new();
    /// <summary>
    /// Gets or sets the delivery performance trend.
    /// </summary>
    public List<PerformanceTrend> DeliveryPerformanceTrend { get; set; } = new();
}

/// <summary>
/// Represents StatMetric.
/// </summary>
public class StatMetric
{
    /// <summary>
    /// Gets or sets the count.
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// Gets or sets the percentage change.
    /// </summary>
    public int PercentageChange { get; set; }
}

/// <summary>
/// Represents DailyShipment.
/// </summary>
public class DailyShipment
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public string Date { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the shipments.
    /// </summary>
    public int Shipments { get; set; }
}

/// <summary>
/// Represents ServiceDistribution.
/// </summary>
public class ServiceDistribution
{
    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the percentage.
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Represents PerformanceTrend.
/// </summary>
public class PerformanceTrend
{
    /// <summary>
    /// Gets or sets the month.
    /// </summary>
    public string Month { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the on time percent.
    /// </summary>
    public double OnTimePercent { get; set; }
    /// <summary>
    /// Gets or sets the delayed percent.
    /// </summary>
    public double DelayedPercent { get; set; }
}



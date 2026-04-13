/// <summary>
/// Provides backend implementation for ReportingDTOs.
/// </summary>

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents DashboardMetricsDTO.
/// </summary>
public class DashboardMetricsDTO
{
    /// <summary>
    /// Gets or sets the total active hubs.
    /// </summary>
    public int TotalActiveHubs { get; set; }
    /// <summary>
    /// Gets or sets the total service locations.
    /// </summary>
    public int TotalServiceLocations { get; set; }
    /// <summary>
    /// Gets or sets the active exceptions.
    /// </summary>
    public int ActiveExceptions { get; set; }
}

/// <summary>
/// Represents ShipmentStatsDTO.
/// </summary>
public class ShipmentStatsDTO
{
    /// <summary>
    /// Gets or sets the total shipments.
    /// </summary>
    public int TotalShipments { get; set; }
    /// <summary>
    /// Gets or sets the in transit.
    /// </summary>
    public int InTransit { get; set; }
    /// <summary>
    /// Gets or sets the delivered.
    /// </summary>
    public int Delivered { get; set; }
}

/// <summary>
/// Represents DateRangeReportRequestDTO.
/// </summary>
public class DateRangeReportRequestDTO
{
    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateTime StartDate { get; set; }
    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateTime EndDate { get; set; }
}



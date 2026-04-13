/// <summary>
/// Provides backend implementation for ReportingDTOs.
/// </summary>

namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Represents DashboardMetricsDTO.
/// </summary>
public class DashboardMetricsDTO
{
    public int TotalActiveHubs { get; set; }
    public int TotalServiceLocations { get; set; }
    public int ActiveExceptions { get; set; }
}

/// <summary>
/// Represents ShipmentStatsDTO.
/// </summary>
public class ShipmentStatsDTO
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Delivered { get; set; }
}

/// <summary>
/// Represents DateRangeReportRequestDTO.
/// </summary>
public class DateRangeReportRequestDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}



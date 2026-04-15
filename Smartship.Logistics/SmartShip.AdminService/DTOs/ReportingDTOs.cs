namespace SmartShip.AdminService.DTOs;

/// <summary>
/// Data transfer model for dashboard metrics payloads.
/// </summary>
public class DashboardMetricsDTO
{
    public int TotalActiveHubs { get; set; }
    public int TotalServiceLocations { get; set; }
    public int ActiveExceptions { get; set; }
}

/// <summary>
/// Data transfer model for shipment stats payloads.
/// </summary>
public class ShipmentStatsDTO
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Delivered { get; set; }
}

/// <summary>
/// Data transfer model for date range report request payloads.
/// </summary>
public class DateRangeReportRequestDTO
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}



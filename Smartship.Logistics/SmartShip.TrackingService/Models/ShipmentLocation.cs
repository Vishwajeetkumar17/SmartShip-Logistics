/// <summary>
/// Provides backend implementation for ShipmentLocation.
/// </summary>

namespace SmartShip.TrackingService.Models;

/// <summary>
/// Represents ShipmentLocation.
/// </summary>
public class ShipmentLocation
{
    public int LocationId { get; set; }
    
    public string TrackingNumber { get; set; } = string.Empty;
    
    public decimal Latitude { get; set; }
    
    public decimal Longitude { get; set; }
    
    public DateTime Timestamp { get; set; }
}



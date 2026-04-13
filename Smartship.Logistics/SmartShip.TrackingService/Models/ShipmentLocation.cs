/// <summary>
/// Provides backend implementation for ShipmentLocation.
/// </summary>

namespace SmartShip.TrackingService.Models;

/// <summary>
/// Represents ShipmentLocation.
/// </summary>
public class ShipmentLocation
{
    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public int LocationId { get; set; }
    
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}



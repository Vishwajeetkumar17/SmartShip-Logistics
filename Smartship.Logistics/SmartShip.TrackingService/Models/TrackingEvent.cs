/// <summary>
/// Provides backend implementation for TrackingEvent.
/// </summary>

namespace SmartShip.TrackingService.Models;

/// <summary>
/// Represents TrackingEvent.
/// </summary>
public class TrackingEvent
{
    /// <summary>
    /// Gets or sets the event id.
    /// </summary>
    public int EventId { get; set; }
    
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}



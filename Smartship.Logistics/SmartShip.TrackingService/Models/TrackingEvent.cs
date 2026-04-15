namespace SmartShip.TrackingService.Models;

/// <summary>
/// Event contract for tracking notifications.
/// </summary>
public class TrackingEvent
{
    public int EventId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}



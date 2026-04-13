/// <summary>
/// Provides backend implementation for TrackingResponseDTO.
/// </summary>

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Represents TrackingResponseDTO.
/// </summary>
public class TrackingResponseDTO
{
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public string CurrentStatus { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the current location.
    /// </summary>
    public string CurrentLocation { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the initial event timestamp.
    /// </summary>
    public DateTime? InitialEventTimestamp { get; set; }
    /// <summary>
    /// Gets or sets the latest event timestamp.
    /// </summary>
    public DateTime? LatestEventTimestamp { get; set; }
    /// <summary>
    /// Gets or sets the timeline.
    /// </summary>
    public List<TrackingEventDTO> Timeline { get; set; } = new();
}



/// <summary>
/// Provides backend implementation for TrackingResponseDTO.
/// </summary>

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Represents TrackingResponseDTO.
/// </summary>
public class TrackingResponseDTO
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string CurrentLocation { get; set; } = string.Empty;
    public DateTime? InitialEventTimestamp { get; set; }
    public DateTime? LatestEventTimestamp { get; set; }
    public List<TrackingEventDTO> Timeline { get; set; } = new();
}



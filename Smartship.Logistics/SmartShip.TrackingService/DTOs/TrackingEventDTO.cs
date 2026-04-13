/// <summary>
/// Provides backend implementation for TrackingEventDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Represents TrackingEventDTO.
/// </summary>
public class TrackingEventDTO
{
    /// <summary>
    /// Gets or sets the event id.
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    [Required]
    [MaxLength(32)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "TrackingNumber is required")]
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Location is required")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}



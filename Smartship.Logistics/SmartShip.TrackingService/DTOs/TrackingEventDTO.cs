using System.ComponentModel.DataAnnotations;

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Data transfer model for tracking event payloads.
/// </summary>
public class TrackingEventDTO
{
    public int? EventId { get; set; }
    [Required]
    [MaxLength(32)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "TrackingNumber is required")]
    public string TrackingNumber { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Location is required")]
    public string Location { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}



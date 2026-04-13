/// <summary>
/// Provides backend implementation for LocationUpdateDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;
using SmartShip.Shared.Common.Helpers;

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Represents LocationUpdateDTO.
/// </summary>
public class LocationUpdateDTO
{
    [Required]
    [MaxLength(32)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "TrackingNumber is required")]
    public string TrackingNumber { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public decimal Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public decimal Longitude { get; set; }

    public DateTime Timestamp { get; set; } = TimeZoneHelper.GetCurrentUtcTime();
}



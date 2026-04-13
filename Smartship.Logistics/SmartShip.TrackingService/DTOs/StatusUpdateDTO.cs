/// <summary>
/// Provides backend implementation for StatusUpdateDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Represents StatusUpdateDTO.
/// </summary>
public class StatusUpdateDTO
{
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    [JsonIgnore]
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
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}



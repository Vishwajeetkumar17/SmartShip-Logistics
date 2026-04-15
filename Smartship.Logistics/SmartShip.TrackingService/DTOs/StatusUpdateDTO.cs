using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartShip.TrackingService.DTOs;

/// <summary>
/// Data transfer model for status update payloads.
/// </summary>
public class StatusUpdateDTO
{
    [JsonIgnore]
    public string TrackingNumber { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}



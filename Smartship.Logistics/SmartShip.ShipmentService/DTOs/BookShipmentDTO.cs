using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for book shipment payloads.
/// </summary>
public class BookShipmentDTO
{
    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "HubName is required")]
    public string HubName { get; set; } = string.Empty;
    [MaxLength(300)]
    public string? HubAddress { get; set; }
}



using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for shipment status update payloads.
/// </summary>
public class ShipmentStatusUpdateDTO
{
    [MaxLength(300)]
    public string? HubLocation { get; set; }
}



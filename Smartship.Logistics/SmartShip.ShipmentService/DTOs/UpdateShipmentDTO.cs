using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for update shipment payloads.
/// </summary>
public class UpdateShipmentDTO
{
    [Range(0, 100000)]
    public decimal TotalWeight { get; set; }
}



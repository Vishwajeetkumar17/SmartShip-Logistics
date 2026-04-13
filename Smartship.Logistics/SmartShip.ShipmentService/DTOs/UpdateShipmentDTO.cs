/// <summary>
/// Provides backend implementation for UpdateShipmentDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents UpdateShipmentDTO.
/// </summary>
public class UpdateShipmentDTO
{
    /// <summary>
    /// Gets or sets the total weight.
    /// </summary>
    [Range(0, 100000)]
    public decimal TotalWeight { get; set; }
}



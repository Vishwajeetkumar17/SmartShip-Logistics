/// <summary>
/// Provides backend implementation for ShipmentStatusUpdateDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents ShipmentStatusUpdateDTO.
/// </summary>
public class ShipmentStatusUpdateDTO
{
    [MaxLength(300)]
    public string? HubLocation { get; set; }
}



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
    /// <summary>
    /// Gets or sets the hub location.
    /// </summary>
    [MaxLength(300)]
    public string? HubLocation { get; set; }
}



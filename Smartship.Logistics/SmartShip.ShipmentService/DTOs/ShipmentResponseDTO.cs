/// <summary>
/// Provides backend implementation for ShipmentResponseDTO.
/// </summary>

using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents ShipmentResponseDTO.
/// </summary>
public class ShipmentResponseDTO
{
    public int ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderPhone { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverPhone { get; set; }
    public string ServiceType { get; set; } = "Standard";
    public ShipmentStatus Status { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; }

    public Address SenderAddress { get; set; } = new();
    public Address ReceiverAddress { get; set; } = new();

    public List<PackageDTO> Packages { get; set; } = new();
    public PickupScheduleDTO? PickupSchedule { get; set; }
}



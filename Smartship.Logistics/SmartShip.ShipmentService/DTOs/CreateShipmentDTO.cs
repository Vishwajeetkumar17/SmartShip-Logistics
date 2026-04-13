/// <summary>
/// Provides backend implementation for CreateShipmentDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents CreateShipmentDTO.
/// </summary>
public class CreateShipmentDTO
{
    [JsonIgnore]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? SenderPhone { get; set; }

    [Required]
    [MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? ReceiverPhone { get; set; }

    [Range(0, 1000000)]
    public decimal? EstimatedCost { get; set; }

    [Required]
    [MaxLength(20)]
    [RegularExpression("^(Standard|Express|Economy)$", ErrorMessage = "ServiceType must be Standard, Express, or Economy")]
    public string ServiceType { get; set; } = "Standard";

    [Required]
    public required Address SenderAddress { get; set; }

    [Required]
    public required Address ReceiverAddress { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one package is required")]
    public List<PackageDTO> Packages { get; set; } = new();

    public PickupScheduleDTO? PickupSchedule { get; set; }
}



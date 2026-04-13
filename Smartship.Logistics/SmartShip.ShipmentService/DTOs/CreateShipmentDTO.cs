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
    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    [JsonIgnore]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender phone.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? SenderPhone { get; set; }

    /// <summary>
    /// Gets or sets the receiver name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the receiver phone.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? ReceiverPhone { get; set; }

    /// <summary>
    /// Gets or sets the estimated cost.
    /// </summary>
    [Range(0, 1000000)]
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [RegularExpression("^(Standard|Express|Economy)$", ErrorMessage = "ServiceType must be Standard, Express, or Economy")]
    public string ServiceType { get; set; } = "Standard";

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    [Required]
    public required Address SenderAddress { get; set; }

    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    [Required]
    public required Address ReceiverAddress { get; set; }

    /// <summary>
    /// Gets or sets the packages.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one package is required")]
    public List<PackageDTO> Packages { get; set; } = new();

    /// <summary>
    /// Gets or sets the pickup schedule.
    /// </summary>
    public PickupScheduleDTO? PickupSchedule { get; set; }
}



using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for create shipment payloads.
/// </summary>
public class CreateShipmentDTO
{
    /// <summary>
    /// Identifier for customer.
    /// </summary>
    [JsonIgnore]
    public int CustomerId { get; set; }

    /// <summary>
    /// Sender Name value.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number used for contact.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? SenderPhone { get; set; }

    /// <summary>
    /// Receiver Name value.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number used for contact.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? ReceiverPhone { get; set; }

    /// <summary>
    /// Estimated Cost value.
    /// </summary>
    [Range(0, 1000000)]
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Service Type value.
    /// </summary>
    [Required]
    [MaxLength(20)]
    [RegularExpression("^(Standard|Express|Economy)$", ErrorMessage = "ServiceType must be Standard, Express, or Economy")]
    public string ServiceType { get; set; } = "Standard";

    /// <summary>
    /// Sender Address value.
    /// </summary>
    [Required]
    public required Address SenderAddress { get; set; }

    /// <summary>
    /// Receiver Address value.
    /// </summary>
    [Required]
    public required Address ReceiverAddress { get; set; }

    /// <summary>
    /// Processes new.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one package is required")]
    public List<PackageDTO> Packages { get; set; } = new();

    /// <summary>
    /// Pickup Schedule value.
    /// </summary>
    public PickupScheduleDTO? PickupSchedule { get; set; }
}



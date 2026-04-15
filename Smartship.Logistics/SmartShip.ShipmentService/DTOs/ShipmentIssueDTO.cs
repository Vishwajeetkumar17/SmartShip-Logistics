using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for shipment issue payloads.
/// </summary>
public class ShipmentIssueDTO
{
    [Required]
    [MaxLength(50)]
    public string IssueType { get; set; } = string.Empty;
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}



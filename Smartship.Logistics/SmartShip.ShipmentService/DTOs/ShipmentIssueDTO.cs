/// <summary>
/// Provides backend implementation for ShipmentIssueDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents ShipmentIssueDTO.
/// </summary>
public class ShipmentIssueDTO
{
    /// <summary>
    /// Gets or sets the issue type.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string IssueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}



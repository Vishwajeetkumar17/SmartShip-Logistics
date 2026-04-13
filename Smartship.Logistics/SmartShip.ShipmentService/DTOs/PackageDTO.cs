/// <summary>
/// Provides backend implementation for PackageDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents PackageDTO.
/// </summary>
public class PackageDTO
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Weight must be greater than 0")]
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Length must be greater than 0")]
    public double Length { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Width must be greater than 0")]
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Height must be greater than 0")]
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}



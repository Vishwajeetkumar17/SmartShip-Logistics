using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for package payloads.
/// </summary>
public class PackageDTO
{
    public int? Id { get; set; }
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Weight must be greater than 0")]
    public decimal Weight { get; set; }
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Length must be greater than 0")]
    public double Length { get; set; }
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Width must be greater than 0")]
    public double Width { get; set; }
    [Required]
    [Range(0.1, 10000, ErrorMessage = "Height must be greater than 0")]
    public double Height { get; set; }
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}



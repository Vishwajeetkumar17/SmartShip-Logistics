using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Data transfer model for delivery proof payloads.
/// </summary>
public class DeliveryProofDTO
{
    [Required]
    public IFormFile? File { get; set; }
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "SignerName is required")]
    public string SignerName { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}



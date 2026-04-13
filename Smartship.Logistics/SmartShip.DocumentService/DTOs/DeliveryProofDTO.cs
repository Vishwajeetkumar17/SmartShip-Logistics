/// <summary>
/// Provides backend implementation for DeliveryProofDTO.
/// </summary>

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Represents DeliveryProofDTO.
/// </summary>
public class DeliveryProofDTO
{
    /// <summary>
    /// Gets or sets the file.
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }

    /// <summary>
    /// Gets or sets the signer name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "SignerName is required")]
    public string SignerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;
}



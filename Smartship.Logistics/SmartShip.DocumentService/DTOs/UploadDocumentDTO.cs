/// <summary>
/// Provides backend implementation for UploadDocumentDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Represents UploadDocumentDTO.
/// </summary>
public class UploadDocumentDTO
{
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ShipmentId must be greater than 0")]
    public int ShipmentId { get; set; }

    /// <summary>
    /// Gets or sets the file.
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }
}



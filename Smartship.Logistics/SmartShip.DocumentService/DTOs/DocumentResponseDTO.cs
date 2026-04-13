/// <summary>
/// Provides backend implementation for DocumentResponseDTO.
/// </summary>

namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Represents DocumentResponseDTO.
/// </summary>
public class DocumentResponseDTO
{
    public int DocumentId { get; set; }
    public int ShipmentId { get; set; }
    public int CustomerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}



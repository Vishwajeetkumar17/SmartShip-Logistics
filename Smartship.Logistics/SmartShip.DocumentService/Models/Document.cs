/// <summary>
/// Provides backend implementation for Document.
/// </summary>

namespace SmartShip.DocumentService.Models;

/// <summary>
/// Represents Document.
/// </summary>
public class Document
{
    public int DocumentId { get; set; }
    
    public int ShipmentId { get; set; }
    
    public int CustomerId { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    
    public string FilePath { get; set; } = string.Empty;
    
    public string DocumentType { get; set; } = string.Empty;
    
    public string ContentType { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; }
}



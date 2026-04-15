namespace SmartShip.DocumentService.Models;

/// <summary>
/// Domain model for document.
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



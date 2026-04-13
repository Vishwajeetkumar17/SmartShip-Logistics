/// <summary>
/// Provides backend implementation for Document.
/// </summary>

namespace SmartShip.DocumentService.Models;

/// <summary>
/// Represents Document.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the document id.
    /// </summary>
    public int DocumentId { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the document type.
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the uploaded at.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}



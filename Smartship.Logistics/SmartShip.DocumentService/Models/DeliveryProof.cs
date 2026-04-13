/// <summary>
/// Provides backend implementation for DeliveryProof.
/// </summary>

namespace SmartShip.DocumentService.Models;

/// <summary>
/// Represents DeliveryProof.
/// </summary>
public class DeliveryProof
{
    /// <summary>
    /// Gets or sets the proof id.
    /// </summary>
    public int ProofId { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the signer name.
    /// </summary>
    public string SignerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }
}



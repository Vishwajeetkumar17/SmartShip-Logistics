/// <summary>
/// Provides backend implementation for DeliveryProof.
/// </summary>

namespace SmartShip.DocumentService.Models;

/// <summary>
/// Represents DeliveryProof.
/// </summary>
public class DeliveryProof
{
    public int ProofId { get; set; }
    
    public int ShipmentId { get; set; }
    
    public string FilePath { get; set; } = string.Empty;
    
    public string SignerName { get; set; } = string.Empty;
    
    public string Notes { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; }
}



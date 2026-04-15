namespace SmartShip.DocumentService.Models;

/// <summary>
/// Domain model for delivery proof.
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



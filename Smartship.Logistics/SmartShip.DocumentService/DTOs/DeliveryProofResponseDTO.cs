namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Data transfer model for delivery proof response payloads.
/// </summary>
public class DeliveryProofResponseDTO
{
    public int ProofId { get; set; }
    public int ShipmentId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}



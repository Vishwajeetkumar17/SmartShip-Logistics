/// <summary>
/// Provides backend implementation for DeliveryProofResponseDTO.
/// </summary>

namespace SmartShip.DocumentService.DTOs;

/// <summary>
/// Represents DeliveryProofResponseDTO.
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



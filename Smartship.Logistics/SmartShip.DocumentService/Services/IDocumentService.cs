/// <summary>
/// Provides backend implementation for IDocumentService.
/// </summary>

using SmartShip.DocumentService.DTOs;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.DTOs;

namespace SmartShip.DocumentService.Services;

/// <summary>
/// Represents IDocumentService.
/// </summary>
public interface IDocumentService
{
    Task<DocumentResponseDTO> UploadDocumentAsync(UploadDocumentDTO dto, string documentType, int customerId);
    Task<DocumentResponseDTO> GetDocumentByIdAsync(int documentId);
    Task<PaginatedResponse<DocumentResponseDTO>> GetDocumentsByShipmentAsync(int shipmentId, int pageNumber = 1, int pageSize = 5);
    Task<List<DocumentResponseDTO>> GetDocumentsByCustomerAsync(int customerId);
    Task<DocumentResponseDTO> UpdateDocumentAsync(int documentId, UploadDocumentDTO dto, int customerId, bool isAdmin);
    Task DeleteDocumentAsync(int documentId);

    Task<DeliveryProofResponseDTO> UploadDeliveryProofAsync(int shipmentId, DeliveryProofDTO dto);
    Task<DeliveryProofResponseDTO> GetDeliveryProofAsync(int shipmentId);

    Task CreateDeliveryConfirmationDocumentAsync(ShipmentDeliveredEvent @event);
}



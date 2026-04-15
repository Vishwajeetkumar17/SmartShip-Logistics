using SmartShip.DocumentService.Models;

namespace SmartShip.DocumentService.Repositories;

/// <summary>
/// Contract for idocument persistence operations.
/// </summary>
public interface IDocumentRepository
{
    Task<Document?> GetDocumentByIdAsync(int documentId);
    Task<List<Document>> GetDocumentsByShipmentAsync(int shipmentId);
    Task<List<Document>> GetDocumentsByCustomerAsync(int customerId);
    Task AddDocumentAsync(Document document);
    Task UpdateDocumentAsync(Document document);
    Task DeleteDocumentAsync(Document document);

    Task<DeliveryProof?> GetDeliveryProofAsync(int shipmentId);
    Task AddDeliveryProofAsync(DeliveryProof proof);
}



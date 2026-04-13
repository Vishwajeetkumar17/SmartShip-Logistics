/// <summary>
/// Provides shipment document upload, retrieval, and delivery proof management workflows.
/// </summary>

using SmartShip.DocumentService.DTOs;
using SmartShip.DocumentService.Helpers;
using SmartShip.DocumentService.Models;
using SmartShip.DocumentService.Repositories;
using SmartShip.DocumentService.Storage;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Extensions;
using SmartShip.Shared.Common.Helpers;
using SmartShip.Shared.DTOs;

namespace SmartShip.DocumentService.Services;

/// <summary>
/// Coordinates shipment document storage, retrieval, and delivery proof processing.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IFileStorageService _storageService;



    #region DocumentService
    /// <summary>
    /// Initializes a new instance of the DocumentService service.
    /// </summary>
    public DocumentService(IDocumentRepository repository, IFileStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }
    #endregion



    #region UploadDocumentAsync
    /// <summary>
    /// Uploads document using service business rules.
    /// </summary>
    public async Task<DocumentResponseDTO> UploadDocumentAsync(UploadDocumentDTO dto, string documentType, int customerId)
    {
        if (customerId <= 0)
        {
            throw new RequestValidationException("Authenticated customer id is required.");
        }

        DocumentValidationHelper.ValidateUpload(dto, documentType);
        documentType = DocumentValidationHelper.NormalizeDocumentType(documentType);

        var folderName = documentType.ToLowerInvariant();
        var fileUrl = await _storageService.SaveFileAsync(dto.File!, folderName);

        var document = new Document
        {
            ShipmentId = dto.ShipmentId,
            CustomerId = customerId,
            FileName = Path.GetFileName(dto.File!.FileName),
            FilePath = fileUrl,
            DocumentType = documentType,
            ContentType = dto.File.ContentType,
            UploadedAt = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddDocumentAsync(document);
        return MapToDto(document);
    }
    #endregion



    #region GetDocumentByIdAsync
    /// <summary>
    /// Retrieves document by id for the current request.
    /// </summary>
    public async Task<DocumentResponseDTO> GetDocumentByIdAsync(int documentId)
    {
        var doc = await _repository.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException($"Document {documentId} not found");

        return MapToDto(doc);
    }
    #endregion



    #region GetDocumentsByShipmentAsync
    /// <summary>
    /// Retrieves documents by shipment for the current request.
    /// </summary>
    public async Task<PaginatedResponse<DocumentResponseDTO>> GetDocumentsByShipmentAsync(int shipmentId, int pageNumber = 1, int pageSize = 5)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        var docs = await _repository.GetDocumentsByShipmentAsync(shipmentId);
        var totalCount = docs.Count;

        var docDtos = docs.Select(MapToDto).ToList();

        var pagedDocs = docDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedDocs.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion



    #region GetDocumentsByCustomerAsync
    /// <summary>
    /// Retrieves documents by customer for the current request.
    /// </summary>
    public async Task<List<DocumentResponseDTO>> GetDocumentsByCustomerAsync(int customerId)
    {
        if (customerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        var docs = await _repository.GetDocumentsByCustomerAsync(customerId);
        return docs.Select(MapToDto).ToList();
    }
    #endregion



    #region UpdateDocumentAsync
    /// <summary>
    /// Updates document using service business rules.
    /// </summary>
    public async Task<DocumentResponseDTO> UpdateDocumentAsync(int documentId, UploadDocumentDTO dto, int customerId, bool isAdmin)
    {
        var existingDoc = await _repository.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException($"Document {documentId} not found");

        if (!isAdmin && existingDoc.CustomerId != customerId)
        {
            throw new RequestValidationException("You are not allowed to update this document.");
        }

        DocumentValidationHelper.ValidateUpload(dto, existingDoc.DocumentType);

        if (dto.ShipmentId != existingDoc.ShipmentId)
        {
            throw new RequestValidationException("ShipmentId cannot be changed for an existing document.");
        }

        await _storageService.DeleteFileAsync(existingDoc.FilePath);
        var newFileUrl = await _storageService.SaveFileAsync(dto.File!, existingDoc.DocumentType.ToLowerInvariant());

        existingDoc.FileName = Path.GetFileName(dto.File!.FileName);
        existingDoc.FilePath = newFileUrl;
        existingDoc.ContentType = dto.File.ContentType;
        existingDoc.UploadedAt = TimeZoneHelper.GetCurrentUtcTime();

        await _repository.UpdateDocumentAsync(existingDoc);
        return MapToDto(existingDoc);
    }
    #endregion



    #region DeleteDocumentAsync
    /// <summary>
    /// Deletes document using service business rules.
    /// </summary>
    public async Task DeleteDocumentAsync(int documentId)
    {
        var existingDoc = await _repository.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException($"Document {documentId} not found");

        await _storageService.DeleteFileAsync(existingDoc.FilePath);
        await _repository.DeleteDocumentAsync(existingDoc);
    }
    #endregion



    #region UploadDeliveryProofAsync
    /// <summary>
    /// Uploads delivery proof using service business rules.
    /// </summary>
    public async Task<DeliveryProofResponseDTO> UploadDeliveryProofAsync(int shipmentId, DeliveryProofDTO dto)
    {
        DocumentValidationHelper.ValidateDeliveryProof(shipmentId, dto);

        var existingProof = await _repository.GetDeliveryProofAsync(shipmentId);
        if (existingProof != null)
        {
            throw new RequestValidationException($"Delivery proof for shipment {shipmentId} already exists.");
        }

        var fileUrl = await _storageService.SaveFileAsync(dto.File!, "proofs");

        var newProof = new DeliveryProof
        {
            ShipmentId = shipmentId,
            FilePath = fileUrl,
            SignerName = DocumentValidationHelper.NormalizeText(dto.SignerName),
            Notes = DocumentValidationHelper.NormalizeText(dto.Notes),
            Timestamp = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddDeliveryProofAsync(newProof);
        return MapToProofDto(newProof);
    }
    #endregion



    #region GetDeliveryProofAsync
    /// <summary>
    /// Retrieves delivery proof for the current request.
    /// </summary>
    public async Task<DeliveryProofResponseDTO> GetDeliveryProofAsync(int shipmentId)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        var proof = await _repository.GetDeliveryProofAsync(shipmentId)
            ?? throw new NotFoundException($"Delivery proof for shipment {shipmentId} not found");

        return MapToProofDto(proof);
    }
    #endregion



    #region CreateDeliveryConfirmationDocumentAsync
    /// <summary>
    /// Creates delivery confirmation document using service business rules.
    /// </summary>
    public async Task CreateDeliveryConfirmationDocumentAsync(ShipmentDeliveredEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var existingDocs = await _repository.GetDocumentsByShipmentAsync(@event.ShipmentId);
        var expectedName = $"{@event.TrackingNumber}-delivery-receipt.csv";

        if (existingDocs.Any(d => d.DocumentType == "DeliveryReceipt" && d.FileName == expectedName))
        {
            return;
        }

        var document = new Document
        {
            ShipmentId = @event.ShipmentId,
            CustomerId = @event.CustomerId,
            FileName = expectedName,
            FilePath = $"events://delivery-confirmation/{@event.TrackingNumber}",
            DocumentType = "DeliveryReceipt",
            ContentType = "text/csv",
            UploadedAt = @event.Timestamp == default ? TimeZoneHelper.GetCurrentUtcTime() : @event.Timestamp
        };

        await _repository.AddDocumentAsync(document);
    }
    #endregion



    #region MapToDto
    /// <summary>
    /// Maps to dto to the corresponding DTO or response model.
    /// </summary>
    private static DocumentResponseDTO MapToDto(Document doc)
    {
        return new DocumentResponseDTO
        {
            DocumentId = doc.DocumentId,
            ShipmentId = doc.ShipmentId,
            CustomerId = doc.CustomerId,
            FileName = doc.FileName,
            FileUrl = doc.FilePath,
            DocumentType = doc.DocumentType,
            ContentType = doc.ContentType,
            UploadedAt = doc.UploadedAt
        };
    }
    #endregion



    #region MapToProofDto
    /// <summary>
    /// Maps to proof dto to the corresponding DTO or response model.
    /// </summary>
    private static DeliveryProofResponseDTO MapToProofDto(DeliveryProof proof)
    {
        return new DeliveryProofResponseDTO
        {
            ProofId = proof.ProofId,
            ShipmentId = proof.ShipmentId,
            FileUrl = proof.FilePath,
            SignerName = proof.SignerName,
            Notes = proof.Notes,
            Timestamp = proof.Timestamp
        };
    }
    #endregion
}





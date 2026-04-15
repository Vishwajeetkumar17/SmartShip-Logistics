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
/// Coordinates document metadata, file storage, delivery proof capture, and related validation rules.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IFileStorageService _storageService;



    #region Constructor
    /// <summary>
    /// Initializes persistence and blob-style file storage for document operations.
    /// </summary>
    public DocumentService(IDocumentRepository repository, IFileStorageService storageService)
    {
        _repository = repository;
        _storageService = storageService;
    }
    #endregion



    #region Public API
    /// <summary>
    /// Uploads document async.
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



    #region Public API
    /// <summary>
    /// Returns document by id async.
    /// </summary>
    public async Task<DocumentResponseDTO> GetDocumentByIdAsync(int documentId)
    {
        var doc = await _repository.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException($"Document {documentId} not found");

        return MapToDto(doc);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns documents by shipment async.
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



    #region Public API
    /// <summary>
    /// Returns documents by customer async.
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



    #region Public API
    /// <summary>
    /// Updates document async.
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



    #region Public API
    /// <summary>
    /// Deletes document async.
    /// </summary>
    public async Task DeleteDocumentAsync(int documentId)
    {
        var existingDoc = await _repository.GetDocumentByIdAsync(documentId)
            ?? throw new NotFoundException($"Document {documentId} not found");

        await _storageService.DeleteFileAsync(existingDoc.FilePath);
        await _repository.DeleteDocumentAsync(existingDoc);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Uploads delivery proof async.
    /// </summary>
    public async Task<DeliveryProofResponseDTO> UploadDeliveryProofAsync(int shipmentId, DeliveryProofDTO dto)
    {
        DocumentValidationHelper.ValidateDeliveryProof(shipmentId, dto);

        var existingProof = await _repository.GetDeliveryProofAsync(shipmentId);
        if (existingProof != null)
        {
            throw new ConflictException($"Delivery proof for shipment {shipmentId} already exists.");
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



    #region Public API
    /// <summary>
    /// Returns delivery proof async.
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



    #region Public API
    /// <summary>
    /// Creates delivery confirmation document async.
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



    #region Private Helpers
    /// <summary>
    /// Maps a persisted document row to the API response shape (includes public file URL path).
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



    #region Private Helpers
    /// <summary>
    /// Maps a delivery proof entity to the response DTO.
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





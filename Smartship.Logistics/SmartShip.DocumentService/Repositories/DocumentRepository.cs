/// <summary>
/// Provides backend implementation for DocumentRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.DocumentService.Data;
using SmartShip.DocumentService.Models;

namespace SmartShip.DocumentService.Repositories;

/// <summary>
/// Represents DocumentRepository.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the document repository class.
    /// </summary>
    public DocumentRepository(DocumentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes the GetDocumentByIdAsync operation.
    /// </summary>
    public async Task<Document?> GetDocumentByIdAsync(int documentId)
    {
        return await _context.Documents.FindAsync(documentId);
    }

    /// <summary>
    /// Executes the GetDocumentsByShipmentAsync operation.
    /// </summary>
    public async Task<List<Document>> GetDocumentsByShipmentAsync(int shipmentId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.ShipmentId == shipmentId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Executes the GetDocumentsByCustomerAsync operation.
    /// </summary>
    public async Task<List<Document>> GetDocumentsByCustomerAsync(int customerId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Executes the AddDocumentAsync operation.
    /// </summary>
    public async Task AddDocumentAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the UpdateDocumentAsync operation.
    /// </summary>
    public async Task UpdateDocumentAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the DeleteDocumentAsync operation.
    /// </summary>
    public async Task DeleteDocumentAsync(Document document)
    {
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the GetDeliveryProofAsync operation.
    /// </summary>
    public async Task<DeliveryProof?> GetDeliveryProofAsync(int shipmentId)
    {
        return await _context.DeliveryProofs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ShipmentId == shipmentId);
    }

    /// <summary>
    /// Executes the AddDeliveryProofAsync operation.
    /// </summary>
    public async Task AddDeliveryProofAsync(DeliveryProof proof)
    {
        await _context.DeliveryProofs.AddAsync(proof);
        await _context.SaveChangesAsync();
    }
}



using Microsoft.EntityFrameworkCore;
using SmartShip.DocumentService.Data;
using SmartShip.DocumentService.Models;

namespace SmartShip.DocumentService.Repositories;

/// <summary>
/// Repository for document data access operations.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _context;

    /// <summary>
    /// Provides persistence operations for document data.
    /// </summary>
    public DocumentRepository(DocumentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns document by id async.
    /// </summary>
    public async Task<Document?> GetDocumentByIdAsync(int documentId)
    {
        return await _context.Documents.FindAsync(documentId);
    }

    /// <summary>
    /// Returns documents by shipment async.
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
    /// Returns documents by customer async.
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
    /// Adds document async.
    /// </summary>
    public async Task AddDocumentAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates document async.
    /// </summary>
    public async Task UpdateDocumentAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes document async.
    /// </summary>
    public async Task DeleteDocumentAsync(Document document)
    {
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Returns delivery proof async.
    /// </summary>
    public async Task<DeliveryProof?> GetDeliveryProofAsync(int shipmentId)
    {
        return await _context.DeliveryProofs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ShipmentId == shipmentId);
    }

    /// <summary>
    /// Adds delivery proof async.
    /// </summary>
    public async Task AddDeliveryProofAsync(DeliveryProof proof)
    {
        await _context.DeliveryProofs.AddAsync(proof);
        await _context.SaveChangesAsync();
    }
}



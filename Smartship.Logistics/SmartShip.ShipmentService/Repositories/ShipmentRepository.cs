/// <summary>
/// Provides backend implementation for ShipmentRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.ShipmentService.Data;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Represents ShipmentRepository.
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the shipment repository class.
    /// </summary>
    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes the GetAllAsync operation.
    /// </summary>
    public async Task<List<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.PickupSchedule)
            .Include(s => s.Packages)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Executes the GetByIdAsync operation.
    /// </summary>
    public async Task<Shipment?> GetByIdAsync(int id)
    {
        return await _context.Shipments
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.PickupSchedule)
            .Include(s => s.Packages)
            .FirstOrDefaultAsync(s => s.ShipmentId == id);
    }

    /// <summary>
    /// Executes the GetByTrackingNumberAsync operation.
    /// </summary>
    public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.Shipments
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.PickupSchedule)
            .Include(s => s.Packages)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
    }

    /// <summary>
    /// Executes the GetByCustomerAsync operation.
    /// </summary>
    public async Task<List<Shipment>> GetByCustomerAsync(int customerId)
    {
        return await _context.Shipments
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.PickupSchedule)
            .Include(s => s.Packages)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Executes the CreateAsync operation.
    /// </summary>
    public async Task CreateAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the UpdateAsync operation.
    /// </summary>
    public async Task UpdateAsync(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the DeleteAsync operation.
    /// </summary>
    public async Task DeleteAsync(Shipment shipment)
    {
        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync();
    }
}



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

    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes GetAllAsync.
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
    /// Executes GetByIdAsync.
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
    /// Executes GetByTrackingNumberAsync.
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
    /// Executes GetByCustomerAsync.
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
    /// Executes CreateAsync.
    /// </summary>
    public async Task CreateAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes UpdateAsync.
    /// </summary>
    public async Task UpdateAsync(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes DeleteAsync.
    /// </summary>
    public async Task DeleteAsync(Shipment shipment)
    {
        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync();
    }
}



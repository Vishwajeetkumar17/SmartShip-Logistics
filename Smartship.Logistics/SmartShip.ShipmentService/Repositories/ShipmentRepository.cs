using Microsoft.EntityFrameworkCore;
using SmartShip.ShipmentService.Data;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Repository for shipment data access operations.
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    #region Fields
    private readonly ShipmentDbContext _context;
    #endregion

    #region Constructor
    /// <summary>
    /// Provides persistence operations for shipment data.
    /// </summary>
    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Query Operations
    /// <summary>
    /// Returns all async.
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
    /// Returns a record by identifier.
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
    /// Returns a shipment by tracking number.
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
    /// Returns shipments for a specific customer.
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
    #endregion

    #region Command Operations
    /// <summary>
    /// Creates async.
    /// </summary>
    public async Task CreateAsync(Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates async.
    /// </summary>
    public async Task UpdateAsync(Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes async.
    /// </summary>
    public async Task DeleteAsync(Shipment shipment)
    {
        _context.Shipments.Remove(shipment);
        await _context.SaveChangesAsync();
    }
    #endregion
}



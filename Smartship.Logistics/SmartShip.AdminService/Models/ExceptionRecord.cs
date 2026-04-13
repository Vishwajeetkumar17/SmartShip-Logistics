/// <summary>
/// Provides backend implementation for ExceptionRecord.
/// </summary>

namespace SmartShip.AdminService.Models;

/// <summary>
/// Represents ExceptionRecord.
/// </summary>
public class ExceptionRecord
{
    /// <summary>
    /// Gets or sets the exception id.
    /// </summary>
    public int ExceptionId { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the exception type.
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty; // e.g., "Open", "Resolved"
    
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the resolved at.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}



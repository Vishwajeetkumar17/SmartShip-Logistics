/// <summary>
/// Provides backend implementation for ExceptionRecord.
/// </summary>

namespace SmartShip.AdminService.Models;

/// <summary>
/// Represents ExceptionRecord.
/// </summary>
public class ExceptionRecord
{
    public int ExceptionId { get; set; }
    
    public int ShipmentId { get; set; }
    
    public string ExceptionType { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty; // e.g., "Open", "Resolved"
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
}



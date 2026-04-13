/// <summary>
/// Provides backend implementation for Hub.
/// </summary>

namespace SmartShip.AdminService.Models;

/// <summary>
/// Represents Hub.
/// </summary>
public class Hub
{
    /// <summary>
    /// Gets or sets the hub id.
    /// </summary>
    public int HubId { get; set; }
    
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the address.
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the contact number.
    /// </summary>
    public string ContactNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the manager name.
    /// </summary>
    public string ManagerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the is active.
    /// </summary>
    public bool IsActive { get; set; }

    // Navigation property
    /// <summary>
    /// Gets or sets the service locations.
    /// </summary>
    public ICollection<ServiceLocation> ServiceLocations { get; set; } = new List<ServiceLocation>();
}



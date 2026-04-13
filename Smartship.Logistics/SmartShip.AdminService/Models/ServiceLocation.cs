/// <summary>
/// Provides backend implementation for ServiceLocation.
/// </summary>

namespace SmartShip.AdminService.Models;

/// <summary>
/// Represents ServiceLocation.
/// </summary>
public class ServiceLocation
{
    /// <summary>
    /// Gets or sets the location id.
    /// </summary>
    public int LocationId { get; set; }
    
    /// <summary>
    /// Gets or sets the hub id.
    /// </summary>
    public int HubId { get; set; }
    
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the zip code.
    /// </summary>
    public string ZipCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the is active.
    /// </summary>
    public bool IsActive { get; set; }

    // Navigation property
    /// <summary>
    /// Gets or sets the hub.
    /// </summary>
    public Hub Hub { get; set; } = null!;
}



namespace SmartShip.AdminService.Models;

/// <summary>
/// Domain model for hub.
/// </summary>
public class Hub
{
    public int HubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation property
    public ICollection<ServiceLocation> ServiceLocations { get; set; } = new List<ServiceLocation>();
}



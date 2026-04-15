namespace SmartShip.AdminService.Models;

/// <summary>
/// Domain model for service location.
/// </summary>
public class ServiceLocation
{
    public int LocationId { get; set; }
    public int HubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation property
    public Hub Hub { get; set; } = null!;
}



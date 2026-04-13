/// <summary>
/// Provides backend implementation for User.
/// </summary>

namespace SmartShip.IdentityService.Models;

/// <summary>
/// Represents User.
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; }
}



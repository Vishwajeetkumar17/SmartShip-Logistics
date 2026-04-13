/// <summary>
/// Provides backend implementation for User.
/// </summary>

namespace SmartShip.IdentityService.Models;

/// <summary>
/// Represents User.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the phone.
    /// </summary>
    public string Phone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the role id.
    /// </summary>
    public int RoleId { get; set; }
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}



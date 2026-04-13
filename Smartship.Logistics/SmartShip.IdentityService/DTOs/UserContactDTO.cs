/// <summary>
/// Provides backend implementation for UserContactDTO.
/// </summary>

namespace SmartShip.IdentityService.DTOs;

/// <summary>
/// Represents UserContactDTO.
/// </summary>
public sealed class UserContactDTO
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
    /// Gets or sets the role.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}



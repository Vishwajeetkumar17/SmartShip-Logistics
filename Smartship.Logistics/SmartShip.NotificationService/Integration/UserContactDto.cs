/// <summary>
/// Provides backend implementation for UserContactDto.
/// </summary>

namespace SmartShip.NotificationService.Integration;

/// <summary>
/// Represents UserContactDto.
/// </summary>
public sealed class UserContactDto
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



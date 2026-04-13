/// <summary>
/// Provides backend implementation for UserContactDTO.
/// </summary>

namespace SmartShip.IdentityService.DTOs;

/// <summary>
/// Represents UserContactDTO.
/// </summary>
public sealed class UserContactDTO
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}



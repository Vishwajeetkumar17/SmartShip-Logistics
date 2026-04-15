namespace SmartShip.IdentityService.DTOs;

/// <summary>
/// Data transfer model for user contact payloads.
/// </summary>
public sealed class UserContactDTO
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}



namespace SmartShip.NotificationService.Integration;

/// <summary>
/// Domain model for user contact dto.
/// </summary>
public sealed class UserContactDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}



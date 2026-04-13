/// <summary>
/// Provides backend implementation for UserEvents.
/// </summary>

namespace SmartShip.EventBus.Contracts;

/// <summary>
/// Represents UserCreatedEvent.
/// </summary>
public sealed record UserCreatedEvent
{
    /// <summary>
    /// Gets or sets the user id.
    /// </summary>
    public int UserId { get; init; }
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public string Role { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; }
}



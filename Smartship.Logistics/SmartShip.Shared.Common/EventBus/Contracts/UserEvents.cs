/// <summary>
/// Provides backend implementation for UserEvents.
/// </summary>

namespace SmartShip.EventBus.Contracts;

/// <summary>
/// Represents UserCreatedEvent.
/// </summary>
public sealed record UserCreatedEvent
{
    public int UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}



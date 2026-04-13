/// <summary>
/// Provides backend implementation for ShipmentEvents.
/// </summary>

namespace SmartShip.EventBus.Contracts;

/// <summary>
/// Represents ShipmentEventBase.
/// </summary>
public abstract record ShipmentEventBase
{
    public int ShipmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public DateTime Timestamp { get; init; }
    public string? HubLocation { get; init; }
}

/// <summary>
/// Represents ShipmentCreatedEvent.
/// </summary>
public sealed record ShipmentCreatedEvent : ShipmentEventBase;
/// <summary>
/// Represents ShipmentBookedEvent.
/// </summary>
public sealed record ShipmentBookedEvent : ShipmentEventBase;
/// <summary>
/// Represents ShipmentPickedUpEvent.
/// </summary>
public sealed record ShipmentPickedUpEvent : ShipmentEventBase;
/// <summary>
/// Represents ShipmentInTransitEvent.
/// </summary>
public sealed record ShipmentInTransitEvent : ShipmentEventBase;
/// <summary>
/// Represents ShipmentOutForDeliveryEvent.
/// </summary>
public sealed record ShipmentOutForDeliveryEvent : ShipmentEventBase;
/// <summary>
/// Represents ShipmentDeliveredEvent.
/// </summary>
public sealed record ShipmentDeliveredEvent : ShipmentEventBase;

/// <summary>
/// Represents ShipmentExceptionEvent.
/// </summary>
public sealed record ShipmentExceptionEvent : ShipmentEventBase
{
    public string ExceptionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Source { get; init; } = "System";
    public int? RaisedByUserId { get; init; }
}



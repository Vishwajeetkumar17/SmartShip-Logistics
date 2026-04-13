/// <summary>
/// Provides backend implementation for ShipmentEvents.
/// </summary>

namespace SmartShip.EventBus.Contracts;

/// <summary>
/// Represents ShipmentEventBase.
/// </summary>
public abstract record ShipmentEventBase
{
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; init; }
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    public string TrackingNumber { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    public int CustomerId { get; init; }
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; }
    /// <summary>
    /// Gets or sets the hub location.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the exception type.
    /// </summary>
    public string ExceptionType { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string Source { get; init; } = "System";
    /// <summary>
    /// Gets or sets the raised by user id.
    /// </summary>
    public int? RaisedByUserId { get; init; }
}



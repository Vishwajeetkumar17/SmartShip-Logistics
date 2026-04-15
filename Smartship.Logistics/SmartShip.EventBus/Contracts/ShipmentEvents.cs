namespace SmartShip.EventBus.Contracts;

/// <summary>
/// Domain model for shipment event base.
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
/// Event contract for shipment created notifications.
/// </summary>
public sealed record ShipmentCreatedEvent : ShipmentEventBase;
/// <summary>
/// Event contract for shipment booked notifications.
/// </summary>
public sealed record ShipmentBookedEvent : ShipmentEventBase;
/// <summary>
/// Event contract for shipment picked up notifications.
/// </summary>
public sealed record ShipmentPickedUpEvent : ShipmentEventBase;
/// <summary>
/// Event contract for shipment in transit notifications.
/// </summary>
public sealed record ShipmentInTransitEvent : ShipmentEventBase;
/// <summary>
/// Event contract for shipment out for delivery notifications.
/// </summary>
public sealed record ShipmentOutForDeliveryEvent : ShipmentEventBase;
/// <summary>
/// Event contract for shipment delivered notifications.
/// </summary>
public sealed record ShipmentDeliveredEvent : ShipmentEventBase;

/// <summary>
/// Event contract for shipment exception notifications.
/// </summary>
public sealed record ShipmentExceptionEvent : ShipmentEventBase
{
    public string ExceptionType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Source { get; init; } = "System";
    public int? RaisedByUserId { get; init; }
}



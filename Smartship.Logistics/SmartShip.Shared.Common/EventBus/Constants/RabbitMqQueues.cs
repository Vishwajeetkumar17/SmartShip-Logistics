namespace SmartShip.EventBus.Constants;

/// <summary>
/// Domain model for rabbit mq queues.
/// </summary>
public static class RabbitMqQueues
{
    public const string UserCreatedQueue = "user-created-queue";
    public const string ShipmentCreatedQueue = "shipment-created-queue";
    public const string ShipmentBookedQueue = "shipment-booked-queue";
    public const string ShipmentPickedUpQueue = "shipment-pickedup-queue";
    public const string ShipmentInTransitQueue = "shipment-intransit-queue";
    public const string ShipmentOutForDeliveryQueue = "shipment-outfordelivery-queue";
    public const string ShipmentDeliveredQueue = "shipment-delivered-queue";
    public const string ShipmentExceptionQueue = "shipment-exception-queue";
}



namespace SmartShip.EventBus.Abstractions;

/// <summary>
/// Contract for event publisher behavior.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(string queueName, T @event, CancellationToken cancellationToken = default)
        where T : class;
}



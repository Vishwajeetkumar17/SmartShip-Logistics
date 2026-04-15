namespace SmartShip.EventBus.Abstractions;

/// <summary>
/// Contract for event consumer behavior.
/// </summary>
public interface IEventConsumer
{
    Task ConsumeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where T : class;
}



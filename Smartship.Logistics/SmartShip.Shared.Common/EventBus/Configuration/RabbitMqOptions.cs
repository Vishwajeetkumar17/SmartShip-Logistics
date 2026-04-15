namespace SmartShip.EventBus.Configuration;

/// <summary>
/// Configuration model for rabbit mq settings.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort PrefetchCount { get; set; } = 10;
    public int PublishMaxRetryAttempts { get; set; } = 3;
    public int ConsumerMaxRetryAttempts { get; set; } = 3;
    public int BaseRetryDelaySeconds { get; set; } = 2;
}



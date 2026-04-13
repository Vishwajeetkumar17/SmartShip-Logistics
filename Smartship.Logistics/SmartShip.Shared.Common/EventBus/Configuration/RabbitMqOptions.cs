/// <summary>
/// Provides backend implementation for RabbitMqOptions.
/// </summary>

namespace SmartShip.EventBus.Configuration;

/// <summary>
/// Represents RabbitMqOptions.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string HostName { get; set; } = "localhost";
    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; } = 5672;
    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = "guest";
    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = "guest";
    /// <summary>
    /// Gets or sets the virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";
    /// <summary>
    /// Gets or sets the prefetch count.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
    /// <summary>
    /// Gets or sets the publish max retry attempts.
    /// </summary>
    public int PublishMaxRetryAttempts { get; set; } = 3;
    /// <summary>
    /// Gets or sets the consumer max retry attempts.
    /// </summary>
    public int ConsumerMaxRetryAttempts { get; set; } = 3;
    /// <summary>
    /// Gets or sets the base retry delay seconds.
    /// </summary>
    public int BaseRetryDelaySeconds { get; set; } = 2;
}



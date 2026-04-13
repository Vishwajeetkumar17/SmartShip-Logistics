/// <summary>
/// Provides backend implementation for SmtpSettings.
/// </summary>

namespace SmartShip.NotificationService.Configurations;

/// <summary>
/// Represents SmtpSettings.
/// </summary>
public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// Gets or sets the host.
    /// </summary>
    public string Host { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; init; } = 587;
    /// <summary>
    /// Gets or sets the enable ssl.
    /// </summary>
    public bool EnableSsl { get; init; } = true;
    /// <summary>
    /// Gets or sets the timeout ms.
    /// </summary>
    public int TimeoutMs { get; init; } = 15000;
    /// <summary>
    /// Gets or sets the use credentials.
    /// </summary>
    public bool UseCredentials { get; init; } = true;
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the from name.
    /// </summary>
    public string FromName { get; init; } = "SmartShip";
    /// <summary>
    /// Gets or sets the from email.
    /// </summary>
    public string FromEmail { get; init; } = string.Empty;
}



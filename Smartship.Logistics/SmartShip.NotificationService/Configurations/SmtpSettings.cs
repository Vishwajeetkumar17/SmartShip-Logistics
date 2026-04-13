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

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;
    public int TimeoutMs { get; init; } = 15000;
    public bool UseCredentials { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromName { get; init; } = "SmartShip";
    public string FromEmail { get; init; } = string.Empty;
}



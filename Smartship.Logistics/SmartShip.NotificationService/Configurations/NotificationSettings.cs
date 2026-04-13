/// <summary>
/// Provides backend implementation for NotificationSettings.
/// </summary>

namespace SmartShip.NotificationService.Configurations;

/// <summary>
/// Represents NotificationSettings.
/// </summary>
public sealed class NotificationSettings
{
    public const string SectionName = "Notification";

    public string InternalApiKey { get; init; } = string.Empty;
    public string AdminEmailsCsv { get; init; } = string.Empty;

    /// <summary>
    /// Executes GetAdminEmails.
    /// </summary>
    public IReadOnlyCollection<string> GetAdminEmails()
    {
        return AdminEmailsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}



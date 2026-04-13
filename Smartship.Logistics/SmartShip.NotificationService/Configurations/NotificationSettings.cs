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

    /// <summary>
    /// Gets or sets the internal api key.
    /// </summary>
    public string InternalApiKey { get; init; } = string.Empty;
    /// <summary>
    /// Gets or sets the admin emails csv.
    /// </summary>
    public string AdminEmailsCsv { get; init; } = string.Empty;

    /// <summary>
    /// Executes the GetAdminEmails operation.
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



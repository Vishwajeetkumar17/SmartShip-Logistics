namespace SmartShip.NotificationService.Configurations;

/// <summary>
/// Configuration model for notification settings.
/// </summary>
public sealed class NotificationSettings
{
    public const string SectionName = "Notification";
    public string InternalApiKey { get; init; } = string.Empty;
    public string AdminEmailsCsv { get; init; } = string.Empty;

    /// <summary>
    /// Returns admin emails.
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



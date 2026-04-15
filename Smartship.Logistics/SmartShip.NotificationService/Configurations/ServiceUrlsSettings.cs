namespace SmartShip.NotificationService.Configurations;

/// <summary>
/// Configuration model for service urls settings.
/// </summary>
public sealed class ServiceUrlsSettings
{
    public const string SectionName = "ServiceUrls";
    public string IdentityService { get; init; } = string.Empty;
}



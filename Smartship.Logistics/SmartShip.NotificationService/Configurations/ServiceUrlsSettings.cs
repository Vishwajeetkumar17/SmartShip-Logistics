/// <summary>
/// Provides backend implementation for ServiceUrlsSettings.
/// </summary>

namespace SmartShip.NotificationService.Configurations;

/// <summary>
/// Represents ServiceUrlsSettings.
/// </summary>
public sealed class ServiceUrlsSettings
{
    public const string SectionName = "ServiceUrls";

    public string IdentityService { get; init; } = string.Empty;
}



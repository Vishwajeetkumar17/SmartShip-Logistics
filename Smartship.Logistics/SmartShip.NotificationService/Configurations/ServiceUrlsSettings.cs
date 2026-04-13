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

    /// <summary>
    /// Gets or sets the identity service.
    /// </summary>
    public string IdentityService { get; init; } = string.Empty;
}



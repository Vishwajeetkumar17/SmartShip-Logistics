/// <summary>
/// Provides backend implementation for InternalServiceAuthSettings.
/// </summary>

namespace SmartShip.IdentityService.Configurations;

/// <summary>
/// Represents InternalServiceAuthSettings.
/// </summary>
public sealed class InternalServiceAuthSettings
{
    public const string SectionName = "InternalServiceAuth";

    /// <summary>
    /// Gets or sets the api key.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;
}



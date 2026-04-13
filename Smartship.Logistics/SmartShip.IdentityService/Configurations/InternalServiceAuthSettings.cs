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

    public string ApiKey { get; init; } = string.Empty;
}



namespace SmartShip.IdentityService.Configurations;

/// <summary>
/// Configuration model for internal service auth settings.
/// </summary>
public sealed class InternalServiceAuthSettings
{
    public const string SectionName = "InternalServiceAuth";
    public string ApiKey { get; init; } = string.Empty;
}



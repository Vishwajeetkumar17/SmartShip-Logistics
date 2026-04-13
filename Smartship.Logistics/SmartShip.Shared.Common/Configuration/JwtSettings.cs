/// <summary>
/// Provides backend implementation for JwtSettings.
/// </summary>

namespace SmartShip.Shared.Common.Configuration;

/// <summary>
/// Represents JwtSettings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the secret.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the audiences.
    /// </summary>
    public List<string> Audiences { get; set; } = [];
    /// <summary>
    /// Gets or sets the expiry minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Executes the GetValidAudiences operation.
    /// </summary>
    public IReadOnlyList<string> GetValidAudiences()
    {
        var audiences = Audiences
            .Where(audience => !string.IsNullOrWhiteSpace(audience))
            .Select(audience => audience.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return audiences;
    }

    /// <summary>
    /// Executes the Validate operation.
    /// </summary>
    public void Validate(bool requireExpiryMinutes = false)
    {
        if (string.IsNullOrWhiteSpace(Secret))
        {
            throw new InvalidOperationException("JwtSettings:Secret is missing in configuration.");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JwtSettings:Issuer is missing in configuration.");
        }

        var validAudiences = GetValidAudiences();
        if (validAudiences.Count == 0)
        {
            throw new InvalidOperationException("JwtSettings:Audiences is missing in configuration.");
        }

        if (requireExpiryMinutes && ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException("JwtSettings:ExpiryMinutes must be greater than 0.");
        }
    }
}



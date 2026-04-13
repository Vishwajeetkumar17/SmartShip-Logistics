/// <summary>
/// Provides backend implementation for JwtSettings.
/// </summary>

namespace SmartShip.Shared.Common.Configuration;

/// <summary>
/// Represents JwtSettings.
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = [];
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Executes GetValidAudiences.
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
    /// Executes Validate.
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



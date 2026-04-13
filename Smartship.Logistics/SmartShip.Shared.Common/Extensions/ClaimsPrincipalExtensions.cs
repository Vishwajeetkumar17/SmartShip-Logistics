/// <summary>
/// Provides backend implementation for ClaimsPrincipalExtensions.
/// </summary>

using System.Security.Claims;
using SmartShip.Shared.Common.Security;

namespace SmartShip.Shared.Common.Extensions;

/// <summary>
/// Represents ClaimsPrincipalExtensions.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Executes TryGetUserId.
    /// </summary>
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
    {
        foreach (var claimType in ClaimTypeConstants.UserIdClaimTypes)
        {
            var rawValue = user.FindFirst(claimType)?.Value;
            if (int.TryParse(rawValue, out userId))
            {
                return true;
            }
        }

        userId = 0;
        return false;
    }

    /// <summary>
    /// Executes TryGetCustomerId.
    /// </summary>
    public static bool TryGetCustomerId(this ClaimsPrincipal user, out int customerId)
    {
        return user.TryGetUserId(out customerId);
    }

    /// <summary>
    /// Executes IsAdmin.
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole("ADMIN");
    }
}



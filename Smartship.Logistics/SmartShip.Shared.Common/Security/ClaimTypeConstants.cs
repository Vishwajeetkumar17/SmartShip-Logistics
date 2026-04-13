/// <summary>
/// Provides backend implementation for ClaimTypeConstants.
/// </summary>

using System.Security.Claims;

namespace SmartShip.Shared.Common.Security;

/// <summary>
/// Represents ClaimTypeConstants.
/// </summary>
public static class ClaimTypeConstants
{
    public static readonly string[] UserIdClaimTypes =
    [
        ClaimTypes.NameIdentifier,
        "sub",
        "id",
        "customerId"
    ];
}



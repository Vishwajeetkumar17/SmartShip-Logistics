using System.Security.Claims;

namespace SmartShip.Shared.Common.Security;

/// <summary>
/// Domain model for claim type constants.
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



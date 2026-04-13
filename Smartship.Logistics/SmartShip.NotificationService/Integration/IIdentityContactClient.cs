/// <summary>
/// Provides backend implementation for IIdentityContactClient.
/// </summary>

namespace SmartShip.NotificationService.Integration;

/// <summary>
/// Represents IIdentityContactClient.
/// </summary>
public interface IIdentityContactClient
{
    /// <summary>
    /// Gets user contact information with optional correlation ID propagation.
    /// </summary>
    /// <param name="userId">The user ID to lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <returns>User contact DTO if found, null otherwise</returns>
    Task<UserContactDto?> GetUserContactAsync(int userId, CancellationToken cancellationToken, string? correlationId = null);
}



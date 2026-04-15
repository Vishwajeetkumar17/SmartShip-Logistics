namespace SmartShip.NotificationService.Integration;

/// <summary>
/// Contract for identity contact client behavior.
/// </summary>
public interface IIdentityContactClient
{
    /// <summary>
    /// Returns user contact async.
    /// </summary>
    /// <param name="userId">The user ID to lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="correlationId">Optional correlation ID for distributed tracing</param>
    /// <returns>User contact DTO if found, null otherwise</returns>
    Task<UserContactDto?> GetUserContactAsync(int userId, CancellationToken cancellationToken, string? correlationId = null);
}



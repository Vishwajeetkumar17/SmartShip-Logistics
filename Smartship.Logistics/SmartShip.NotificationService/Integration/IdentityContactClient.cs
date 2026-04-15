using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartShip.NotificationService.Configurations;

namespace SmartShip.NotificationService.Integration;

/// <summary>
/// Domain model for identity contact client.
/// </summary>
public sealed class IdentityContactClient : IIdentityContactClient
{
    private readonly HttpClient _httpClient;
    private readonly NotificationSettings _notificationSettings;
    private readonly ILogger<IdentityContactClient> _logger;

    /// <summary>
    /// Processes identity contact client.
    /// </summary>
    public IdentityContactClient(
        HttpClient httpClient,
        IOptions<NotificationSettings> notificationOptions,
        ILogger<IdentityContactClient> logger)
    {
        _httpClient = httpClient;
        _notificationSettings = notificationOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Returns user contact async.
    /// </summary>
    public async Task<UserContactDto?> GetUserContactAsync(int userId, CancellationToken cancellationToken, string? correlationId = null)
    {
        if (userId <= 0)
        {
            return null;
        }

        var apiKey = _notificationSettings.InternalApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Notification internal API key is not configured. User contact lookup skipped for userId {UserId}", userId);
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/auth/internal/users/{userId}/contact");
        request.Headers.Add("X-Internal-Api-Key", apiKey);

        // ✓ Propagate Correlation ID for distributed tracing
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call identity contact endpoint for userId {UserId}", userId);
            throw;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Identity contact endpoint returned {(int)response.StatusCode} for userId {userId}. Body: {body}");
        }

        return await response.Content.ReadFromJsonAsync<UserContactDto>(cancellationToken: cancellationToken);
    }
}



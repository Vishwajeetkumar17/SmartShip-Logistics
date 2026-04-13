/// <summary>
/// Provides backend implementation for IEmailNotificationService.
/// </summary>

namespace SmartShip.NotificationService.Services;

/// <summary>
/// Represents IEmailNotificationService.
/// </summary>
public interface IEmailNotificationService
{
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken);
}



namespace SmartShip.NotificationService.Services;

/// <summary>
/// Defines email notification business operations used by the service layer.
/// </summary>
public interface IEmailNotificationService
{
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken);
}



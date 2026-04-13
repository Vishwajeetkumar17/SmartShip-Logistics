/// <summary>
/// Provides backend implementation for SmtpEmailNotificationService.
/// </summary>

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartShip.NotificationService.Configurations;

namespace SmartShip.NotificationService.Services;

/// <summary>
/// Represents SmtpEmailNotificationService.
/// </summary>
public sealed class SmtpEmailNotificationService : IEmailNotificationService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<SmtpEmailNotificationService> _logger;

    public SmtpEmailNotificationService(
        IOptions<SmtpSettings> smtpOptions,
        ILogger<SmtpEmailNotificationService> logger)
    {
        _smtpSettings = smtpOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Executes SendEmailAsync.
    /// </summary>
    public async Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken)
    {
        var recipientList = recipients
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipientList.Count == 0)
        {
            return;
        }

        EnsureSmtpConfigured();

        using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
        {
            EnableSsl = _smtpSettings.EnableSsl,
            Timeout = _smtpSettings.TimeoutMs,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (_smtpSettings.UseCredentials)
        {
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username.Trim(), _smtpSettings.Password.Trim());
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail.Trim(), _smtpSettings.FromName.Trim()),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        foreach (var recipient in recipientList)
        {
            message.To.Add(recipient);
        }

        using var timeoutCts = new CancellationTokenSource(_smtpSettings.TimeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        await smtpClient.SendMailAsync(message, linkedCts.Token);
        _logger.LogInformation("Notification email sent to {RecipientCount} recipients", recipientList.Count);
    }

    private void EnsureSmtpConfigured()
    {
        if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
        {
            throw new InvalidOperationException("SMTP Host is not configured for notification service.");
        }

        if (string.IsNullOrWhiteSpace(_smtpSettings.FromEmail))
        {
            throw new InvalidOperationException("SMTP FromEmail is not configured for notification service.");
        }

        if (_smtpSettings.UseCredentials && (string.IsNullOrWhiteSpace(_smtpSettings.Username) || string.IsNullOrWhiteSpace(_smtpSettings.Password)))
        {
            throw new InvalidOperationException("SMTP credentials are required but Username/Password are not configured.");
        }
    }
}


}



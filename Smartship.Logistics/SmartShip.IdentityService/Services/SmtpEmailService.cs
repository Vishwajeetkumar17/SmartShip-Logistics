/// <summary>
/// Provides backend implementation for SmtpEmailService.
/// </summary>

using Microsoft.Extensions.Options;
using SmartShip.IdentityService.Configurations;
using System.Net;
using System.Net.Mail;

namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Represents SmtpEmailService.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpSettings> smtpOptions, ILogger<SmtpEmailService> logger)
        {
            _smtpSettings = smtpOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Executes SendSignupOtpEmailAsync.
        /// </summary>
        public async Task SendSignupOtpEmailAsync(string email, string otp)
        {
            var subject = "SmartShip signup verification OTP";
            var body = $"""
                Hello,

                Your SmartShip signup OTP is: {otp}

                This OTP is valid for 10 minutes.
                If you did not request signup, please ignore this email.

                - SmartShip Team
                """;

            await SendEmailAsync(email, subject, body, "signup OTP");
        }

        /// <summary>
        /// Executes SendPasswordResetEmailAsync.
        /// </summary>
        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var subject = "SmartShip password reset OTP";
            var body = $"""
                Hello,

                Your SmartShip password reset OTP is: {token}

                This OTP is valid for 10 minutes.
                If you did not request this, please ignore this email.

                - SmartShip Team
                """;

            await SendEmailAsync(email, subject, body, "password reset OTP");
        }

        private async Task SendEmailAsync(string email, string subject, string body, string emailPurpose)
        {
            if (string.IsNullOrWhiteSpace(_smtpSettings.Host))
            {
                throw new InvalidOperationException("SMTP Host is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_smtpSettings.FromEmail))
            {
                throw new InvalidOperationException("SMTP FromEmail is not configured.");
            }

            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Timeout = _smtpSettings.TimeoutMs,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (_smtpSettings.UseCredentials)
            {
                if (string.IsNullOrWhiteSpace(_smtpSettings.Username) || string.IsNullOrWhiteSpace(_smtpSettings.Password))
                {
                    throw new InvalidOperationException("SMTP credentials are required but Username/Password are not configured.");
                }

                var username = _smtpSettings.Username.Trim();
                var password = _smtpSettings.Password.Trim();
                if (_smtpSettings.Host.Contains("gmail", StringComparison.OrdinalIgnoreCase))
                {
                    password = password.Replace(" ", string.Empty);

                    if (!username.Contains('@'))
                    {
                        throw new InvalidOperationException("For Gmail SMTP, Smtp:Username must be your full Gmail address.");
                    }

                    if (!string.Equals(_smtpSettings.FromEmail.Trim(), username, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("For Gmail SMTP, Smtp:FromEmail should match Smtp:Username unless a verified alias is configured.");
                    }
                }

                smtpClient.Credentials = new NetworkCredential(username, password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(email);

            using var timeoutCts = new CancellationTokenSource(_smtpSettings.TimeoutMs);

            try
            {
                await smtpClient.SendMailAsync(message, timeoutCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException($"SMTP send timed out after {_smtpSettings.TimeoutMs} ms.");
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP send failed for {Email}", email);
                throw new InvalidOperationException("Unable to send email. Verify SMTP username/password and sender email configuration.");
            }

            _logger.LogInformation("SmartShip {EmailPurpose} email sent to {Email}", emailPurpose, email);
        }
    }
}



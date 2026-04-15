namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Development-friendly email implementation that logs OTP and reset payloads instead of sending mail.
    /// </summary>
    public class LoggingEmailService : IEmailService
    {
        private readonly ILogger<LoggingEmailService> _logger;

        public LoggingEmailService(ILogger<LoggingEmailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends signup otp email async.
        /// </summary>
        public Task SendSignupOtpEmailAsync(string email, string otp)
        {
            _logger.LogInformation("Signup OTP for {Email}: {Otp}", email, otp);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends password reset email async.
        /// </summary>
        public Task SendPasswordResetEmailAsync(string email, string token)
        {
            _logger.LogInformation("Password reset OTP for {Email}: {Otp}", email, token);
            return Task.CompletedTask;
        }
    }
}


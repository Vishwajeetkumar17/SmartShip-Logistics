/// <summary>
/// Provides backend implementation for LoggingEmailService.
/// </summary>

namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Represents LoggingEmailService.
    /// </summary>
    public class LoggingEmailService : IEmailService
    {
        private readonly ILogger<LoggingEmailService> _logger;

        public LoggingEmailService(ILogger<LoggingEmailService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes SendSignupOtpEmailAsync.
        /// </summary>
        public Task SendSignupOtpEmailAsync(string email, string otp)
        {
            _logger.LogInformation("Signup OTP for {Email}: {Otp}", email, otp);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes SendPasswordResetEmailAsync.
        /// </summary>
        public Task SendPasswordResetEmailAsync(string email, string token)
        {
            _logger.LogInformation("Password reset OTP for {Email}: {Otp}", email, token);
            return Task.CompletedTask;
        }
    }
}


}



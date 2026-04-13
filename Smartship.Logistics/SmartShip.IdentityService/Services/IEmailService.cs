/// <summary>
/// Provides backend implementation for IEmailService.
/// </summary>

namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Represents IEmailService.
    /// </summary>
    public interface IEmailService
    {
        Task SendSignupOtpEmailAsync(string email, string otp);
        Task SendPasswordResetEmailAsync(string email, string token);
    }
}



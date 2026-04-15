namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Defines email business operations used by the service layer.
    /// </summary>
    public interface IEmailService
    {
        Task SendSignupOtpEmailAsync(string email, string otp);
        Task SendPasswordResetEmailAsync(string email, string token);
    }
}



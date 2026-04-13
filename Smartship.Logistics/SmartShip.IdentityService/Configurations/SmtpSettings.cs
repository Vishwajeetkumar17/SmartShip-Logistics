/// <summary>
/// Provides backend implementation for SmtpSettings.
/// </summary>

namespace SmartShip.IdentityService.Configurations
{
    /// <summary>
    /// Represents SmtpSettings.
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public int TimeoutMs { get; set; } = 15000;
        public bool UseCredentials { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "SmartShip";
    }
}



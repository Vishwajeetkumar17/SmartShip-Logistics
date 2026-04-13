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
        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; } = 587;
        /// <summary>
        /// Gets or sets the enable ssl.
        /// </summary>
        public bool EnableSsl { get; set; } = true;
        /// <summary>
        /// Gets or sets the timeout ms.
        /// </summary>
        public int TimeoutMs { get; set; } = 15000;
        /// <summary>
        /// Gets or sets the use credentials.
        /// </summary>
        public bool UseCredentials { get; set; } = true;
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the from email.
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the from name.
        /// </summary>
        public string FromName { get; set; } = "SmartShip";
    }
}



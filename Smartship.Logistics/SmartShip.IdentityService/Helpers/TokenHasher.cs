using System.Security.Cryptography;
using System.Text;

namespace SmartShip.IdentityService.Helpers
{
    /// <summary>
    /// Hex-encoded SHA-256 hashing for OTPs and refresh tokens at rest (not for passwords).
    /// </summary>
    public static class TokenHasher
    {
        /// <summary>
        /// Returns a fixed-length hex string hash of the input value.
        /// </summary>
        public static string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}



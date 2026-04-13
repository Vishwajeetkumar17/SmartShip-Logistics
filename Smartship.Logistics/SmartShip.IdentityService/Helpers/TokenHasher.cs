/// <summary>
/// Provides backend implementation for TokenHasher.
/// </summary>

using System.Security.Cryptography;
using System.Text;

namespace SmartShip.IdentityService.Helpers
{
    /// <summary>
    /// Represents TokenHasher.
    /// </summary>
    public static class TokenHasher
    {
        /// <summary>
        /// Executes the Hash operation.
        /// </summary>
        public static string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}



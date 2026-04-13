/// <summary>
/// Provides backend implementation for PasswordHasher.
/// </summary>

using System.Security.Cryptography;
using System.Text;

namespace SmartShip.IdentityService.Helpers
{
    /// <summary>
    /// Represents PasswordHasher.
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Executes the Hash operation.
        /// </summary>
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
        /// <summary>
        /// Executes the Verify operation.
        /// </summary>
        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}



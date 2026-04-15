using System.Security.Cryptography;
using System.Text;

namespace SmartShip.IdentityService.Helpers
{
    /// <summary>
    /// SHA-256 based password hashing for stored credentials (verify compares hashes).
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Returns a Base64 hash of the UTF-8 password bytes.
        /// </summary>
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
        /// <summary>
        /// Returns true when the password hashes to the same value as the stored hash.
        /// </summary>
        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}



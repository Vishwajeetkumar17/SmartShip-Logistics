/// <summary>
/// Provides backend implementation for JwtTokenGenerator.
/// </summary>

using Microsoft.IdentityModel.Tokens;
using SmartShip.Shared.Common.Configuration;
using SmartShip.Shared.Common.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartShip.IdentityService.Security
{
    /// <summary>
    /// Represents JwtTokenGenerator.
    /// </summary>
    public class JwtTokenGenerator
    {
        private const string SigningKeyId = "smartship-jwt-signing-key";
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(JwtSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Executes GenerateToken.
        /// </summary>
        public string GenerateToken(int userId, string email, string role)
        {
            var primaryAudience = _settings.GetValidAudiences()[0];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret))
            {
                KeyId = SigningKeyId
            };

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: primaryAudience,
                claims: claims,
                expires: TimeZoneHelper.GetCurrentUtcTime().AddMinutes(_settings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}



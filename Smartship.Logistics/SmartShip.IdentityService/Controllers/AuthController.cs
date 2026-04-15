using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SmartShip.IdentityService.Configurations;
using SmartShip.IdentityService.DTOs;
using SmartShip.IdentityService.Services;
using System.Security.Claims;

namespace SmartShip.IdentityService.Controllers
{
    [ApiController]
    [Route("auth")]
    /// <summary>
    /// Authentication and account API: registration, login, tokens, passwords, profiles, and admin user management.
    /// </summary>
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly InternalServiceAuthSettings _internalServiceAuth;

        /// <summary>
        /// Initializes the controller with identity services and internal-service auth settings.
        /// </summary>
        public AuthController(IAuthService service, IOptions<InternalServiceAuthSettings> internalServiceAuth)
        {
            _service = service;
            _internalServiceAuth = internalServiceAuth.Value;
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("signup")]
        /// <summary>
        /// Sends a one-time code to the registrant's email so signup can be completed after verification.
        /// </summary>
        public async Task<IActionResult> RequestSignupOtp([FromBody] RegisterDTO dto)
        {
            await _service.RequestSignupOtpAsync(dto);
            return Ok(new { Message = "OTP sent to your email. Verify OTP to complete signup." });
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("signup/verify-otp")]
        /// <summary>
        /// Completes email-verified registration and returns access and refresh tokens.
        /// </summary>
        public async Task<IActionResult> VerifySignupOtp([FromBody] VerifySignupOtpDTO dto)
        {
            return Ok(await _service.VerifySignupOtpAndRegisterAsync(dto));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("google-signup")]
        /// <summary>
        /// Registers or signs in a user using a validated Google ID token.
        /// </summary>
        public async Task<IActionResult> GoogleSignup([FromBody] GoogleSignupDTO dto)
        {
            return Ok(await _service.GoogleSignupAsync(dto));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("login")]
        /// <summary>
        /// Authenticates credentials and returns JWT access and refresh tokens.
        /// </summary>
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            return Ok(await _service.LoginAsync(dto));
        }

        [Authorize]
        [HttpPost("logout")]
        /// <summary>
        /// Revokes the current user's refresh token session (caller must be authenticated).
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _service.LogoutAsync(userId.Value);
            return Ok();
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("refresh-token")]
        /// <summary>
        /// Issues a new access token (and rotated refresh token) using a valid refresh token.
        /// </summary>
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO dto)
        {
            return Ok(await _service.RefreshTokenAsync(dto.RefreshToken));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("forgot-password")]
        /// <summary>
        /// Sends a password reset link or code to the account email if it exists.
        /// </summary>
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            await _service.ForgotPasswordAsync(dto.Email);
            return Ok();
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("reset-password")]
        /// <summary>
        /// Sets a new password using a valid reset token from the forgot-password flow.
        /// </summary>
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            await _service.ResetPasswordAsync(dto);
            return Ok();
        }

        [Authorize]
        [HttpGet("profile")]
        /// <summary>
        /// Returns the authenticated user's profile (name, email, role).
        /// </summary>
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            return Ok(await _service.GetProfileAsync(userId.Value));
        }

        [Authorize]
        [HttpPut("profile")]
        /// <summary>
        /// Updates editable profile fields for the authenticated user.
        /// </summary>
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _service.UpdateProfileAsync(userId.Value, dto);
            return Ok();
        }

        [Authorize]
        [HttpPut("change-password")]
        /// <summary>
        /// Changes the authenticated user's password after verifying the current password.
        /// </summary>
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _service.ChangePasswordAsync(userId.Value, dto);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("users")]
        /// <summary>
        /// Returns a paginated directory of users (admin only).
        /// </summary>
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            return Ok(await _service.GetUsersAsync(pageNumber, pageSize));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("users/{id:int}")]
        /// <summary>
        /// Returns a single user by identifier (admin only).
        /// </summary>
        public async Task<IActionResult> GetUserById(int id)
        {
            return Ok(await _service.GetUserByIdAsync(id));
        }

        [AllowAnonymous]
        [HttpGet("internal/users/{id:int}/contact")]
        /// <summary>
        /// Returns minimal contact info for a user; restricted to trusted services via X-Internal-Api-Key.
        /// </summary>
        public async Task<IActionResult> GetUserContactInternal(
            int id,
            [FromHeader(Name = "X-Internal-Api-Key")] string? apiKey)
        {
            var expectedApiKey = _internalServiceAuth.ApiKey?.Trim();
            if (string.IsNullOrWhiteSpace(expectedApiKey))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal service auth key is not configured.");
            }

            if (!string.Equals(expectedApiKey, apiKey?.Trim(), StringComparison.Ordinal))
            {
                return Unauthorized();
            }

            var user = await _service.GetUserByIdAsync(id);
            return Ok(new UserContactDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("users")]
        /// <summary>
        /// Creates a user account as an administrator (admin only).
        /// </summary>
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            await _service.CreateUserAsync(dto);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("users/{id:int}/resend-welcome")]
        /// <summary>
        /// Triggers another welcome or onboarding email for the specified user (admin only).
        /// </summary>
        public async Task<IActionResult> ResendWelcomeEmail(int id)
        {
            await _service.ResendWelcomeEmailAsync(id);
            return Ok(new { Message = "Welcome email resend requested." });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("users/{id:int}")]
        /// <summary>
        /// Updates profile and role-related fields for a user (admin only).
        /// </summary>
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            await _service.UpdateUserAsync(id, dto);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("users/{id:int}")]
        /// <summary>
        /// Deletes a user account by identifier (admin only).
        /// </summary>
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _service.DeleteUserAsync(id);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("roles")]
        /// <summary>
        /// Returns a paginated list of role definitions (admin only).
        /// </summary>
        public async Task<IActionResult> GetRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            return Ok(await _service.GetRolesAsync(pageNumber, pageSize));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("roles")]
        /// <summary>
        /// Creates a new role (admin only).
        /// </summary>
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO dto)
        {
            return Ok(await _service.CreateRoleAsync(dto));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("users/{id:int}/role")]
        /// <summary>
        /// Assigns a role to a user (admin only).
        /// </summary>
        public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleDTO dto)
        {
            await _service.AssignRoleAsync(id, dto.RoleId);
            return Ok();
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}



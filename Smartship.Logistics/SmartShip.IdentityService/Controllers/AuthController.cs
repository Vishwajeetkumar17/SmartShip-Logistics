/// <summary>
/// Provides backend implementation for AuthController.
/// </summary>

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
    /// Represents AuthController.
    /// </summary>
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly InternalServiceAuthSettings _internalServiceAuth;

        public AuthController(IAuthService service, IOptions<InternalServiceAuthSettings> internalServiceAuth)
        {
            _service = service;
            _internalServiceAuth = internalServiceAuth.Value;
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("signup")]
        /// <summary>
        /// Executes RequestSignupOtp.
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
        /// Executes VerifySignupOtp.
        /// </summary>
        public async Task<IActionResult> VerifySignupOtp([FromBody] VerifySignupOtpDTO dto)
        {
            return Ok(await _service.VerifySignupOtpAndRegisterAsync(dto));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("google-signup")]
        /// <summary>
        /// Executes GoogleSignup.
        /// </summary>
        public async Task<IActionResult> GoogleSignup([FromBody] GoogleSignupDTO dto)
        {
            return Ok(await _service.GoogleSignupAsync(dto));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("login")]
        /// <summary>
        /// Executes Login.
        /// </summary>
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            return Ok(await _service.LoginAsync(dto));
        }

        [Authorize]
        [HttpPost("logout")]
        /// <summary>
        /// Executes Logout.
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
        /// Executes RefreshToken.
        /// </summary>
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO dto)
        {
            return Ok(await _service.RefreshTokenAsync(dto.RefreshToken));
        }

        [AllowAnonymous]
        [EnableRateLimiting("AuthSensitive")]
        [HttpPost("forgot-password")]
        /// <summary>
        /// Executes ForgotPassword.
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
        /// Executes ResetPassword.
        /// </summary>
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            await _service.ResetPasswordAsync(dto);
            return Ok();
        }

        [Authorize]
        [HttpGet("profile")]
        /// <summary>
        /// Executes Profile.
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
        /// Executes UpdateProfile.
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
        /// Executes ChangePassword.
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
        /// Executes GetUsers.
        /// </summary>
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            return Ok(await _service.GetUsersAsync(pageNumber, pageSize));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("users/{id:int}")]
        /// <summary>
        /// Executes GetUserById.
        /// </summary>
        public async Task<IActionResult> GetUserById(int id)
        {
            return Ok(await _service.GetUserByIdAsync(id));
        }

        [AllowAnonymous]
        [HttpGet("internal/users/{id:int}/contact")]
        /// <summary>
        /// Executes GetUserContactInternal.
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
        /// Executes CreateUser.
        /// </summary>
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            await _service.CreateUserAsync(dto);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("users/{id:int}/resend-welcome")]
        /// <summary>
        /// Executes ResendWelcomeEmail.
        /// </summary>
        public async Task<IActionResult> ResendWelcomeEmail(int id)
        {
            await _service.ResendWelcomeEmailAsync(id);
            return Ok(new { Message = "Welcome email resend requested." });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("users/{id:int}")]
        /// <summary>
        /// Executes UpdateUser.
        /// </summary>
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
        {
            await _service.UpdateUserAsync(id, dto);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("users/{id:int}")]
        /// <summary>
        /// Executes DeleteUser.
        /// </summary>
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _service.DeleteUserAsync(id);
            return Ok();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("roles")]
        /// <summary>
        /// Executes GetRoles.
        /// </summary>
        public async Task<IActionResult> GetRoles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            return Ok(await _service.GetRolesAsync(pageNumber, pageSize));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("roles")]
        /// <summary>
        /// Executes CreateRole.
        /// </summary>
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO dto)
        {
            return Ok(await _service.CreateRoleAsync(dto));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("users/{id:int}/role")]
        /// <summary>
        /// Executes AssignRole.
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



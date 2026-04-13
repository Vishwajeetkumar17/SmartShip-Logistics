/// <summary>
/// Provides backend implementation for IAuthService.
/// </summary>

using SmartShip.IdentityService.DTOs;
using SmartShip.Shared.DTOs;

namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Represents IAuthService.
    /// </summary>
    public interface IAuthService
    {
        Task RequestSignupOtpAsync(RegisterDTO dto);
        Task<AuthDTO> VerifySignupOtpAndRegisterAsync(VerifySignupOtpDTO dto);
        Task<AuthDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthDTO> GoogleSignupAsync(GoogleSignupDTO dto);
        Task<AuthDTO> LoginAsync(LoginDTO dto);
        Task LogoutAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO dto);
        Task<AuthDTO> GetProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateProfileDTO dto);

        Task<PaginatedResponse<AuthDTO>> GetUsersAsync(int pageNumber = 1, int pageSize = 5);
        Task<AuthDTO> GetUserByIdAsync(int id);
        Task CreateUserAsync(CreateUserDTO dto);
        Task ResendWelcomeEmailAsync(int id);
        Task UpdateUserAsync(int id, UpdateUserDTO dto);
        Task AssignRoleAsync(int id, int roleId);
        Task DeleteUserAsync(int id);

        Task<PaginatedResponse<RoleDTO>> GetRolesAsync(int pageNumber = 1, int pageSize = 5);
        Task<RoleDTO> CreateRoleAsync(CreateRoleDTO dto);

        Task<AuthDTO> RefreshTokenAsync(string refreshToken);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDTO dto);
    }
}



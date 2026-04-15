using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using SmartShip.IdentityService.Data;
using SmartShip.IdentityService.Configurations;
using SmartShip.IdentityService.DTOs;
using SmartShip.Shared.Common.Helpers;
using SmartShip.IdentityService.Helpers;
using SmartShip.IdentityService.Models;
using SmartShip.IdentityService.Repositories;
using SmartShip.IdentityService.Security;
using SmartShip.Shared.Common.Extensions;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.DTOs;
using System.Security.Cryptography;

namespace SmartShip.IdentityService.Services
{
    /// <summary>
    /// Implements registration, credential verification, JWT and refresh tokens, password flows, and admin user management.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly JwtTokenGenerator _jwt;
        private readonly IdentityDbContext _context;
        private readonly IEmailService _emailService;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMemoryCache _memoryCache;

        private const int SignupOtpExpiryMinutes = 10;
        private const int SignupOtpMaxAttempts = 5;

        #region Construction
        /// <summary>
        /// Wires repositories, JWT issuance, email delivery, Google verification, event publishing, and OTP cache.
        /// </summary>
        public AuthService(
            IUserRepository repository,
            JwtTokenGenerator jwt,
            IdentityDbContext context,
            IEmailService emailService,
            IOptions<GoogleAuthSettings> googleAuthOptions,
            IEventPublisher eventPublisher,
            IMemoryCache memoryCache)
        {
            _repository = repository;
            _jwt = jwt;
            _context = context;
            _emailService = emailService;
            _googleAuthSettings = googleAuthOptions.Value;
            _eventPublisher = eventPublisher;
            _memoryCache = memoryCache;
        }
        #endregion
        #region RequestSignupOtpAsync
        /// <summary>
        /// Stores a pending signup with hashed OTP and emails a verification code (conflicts if email exists).
        /// </summary>
        public async Task RequestSignupOtpAsync(RegisterDTO dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            var existingUser = await _repository.GetByEmailAsync(normalizedEmail);
            if (existingUser != null)
                throw new ConflictException("User already exists");

            await EnsureRoleExistsAsync(dto.RoleId);

            var otp = CreateOtp();
            var otpHash = TokenHasher.Hash(otp);
            var cacheKey = GetSignupOtpCacheKey(normalizedEmail);

            var entry = new PendingSignupOtp
            {
                Name = dto.Name.Trim(),
                Email = normalizedEmail,
                Phone = dto.Phone.Trim(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId,
                OtpHash = otpHash,
                ExpiresAt = TimeZoneHelper.GetCurrentUtcTime().AddMinutes(SignupOtpExpiryMinutes),
                RemainingAttempts = SignupOtpMaxAttempts
            };

            _memoryCache.Set(
                cacheKey,
                entry,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(SignupOtpExpiryMinutes)
                });

            await _emailService.SendSignupOtpEmailAsync(normalizedEmail, otp);
        }
        #endregion
        #region VerifySignupOtpAndRegisterAsync
        /// <summary>
        /// Validates the signup OTP, creates the user, clears cache state, and returns auth tokens.
        /// </summary>
        public async Task<AuthDTO> VerifySignupOtpAndRegisterAsync(VerifySignupOtpDTO dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            var cacheKey = GetSignupOtpCacheKey(normalizedEmail);

            if (!_memoryCache.TryGetValue(cacheKey, out PendingSignupOtp? pendingSignup) || pendingSignup == null)
            {
                throw new RequestValidationException("OTP is invalid or expired");
            }

            if (pendingSignup.ExpiresAt < TimeZoneHelper.GetCurrentUtcTime())
            {
                _memoryCache.Remove(cacheKey);
                throw new RequestValidationException("OTP is invalid or expired");
            }

            var otpHash = TokenHasher.Hash(dto.Otp.Trim());
            if (!string.Equals(otpHash, pendingSignup.OtpHash, StringComparison.Ordinal))
            {
                pendingSignup.RemainingAttempts--;
                if (pendingSignup.RemainingAttempts <= 0)
                {
                    _memoryCache.Remove(cacheKey);
                }
                else
                {
                    _memoryCache.Set(
                        cacheKey,
                        pendingSignup,
                        new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = pendingSignup.ExpiresAt
                        });
                }

                throw new RequestValidationException("Invalid OTP");
            }

            var existingUser = await _repository.GetByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                _memoryCache.Remove(cacheKey);
                throw new ConflictException("User already exists");
            }

            var user = new User
            {
                Name = pendingSignup.Name,
                Email = pendingSignup.Email,
                Phone = pendingSignup.Phone,
                PasswordHash = pendingSignup.PasswordHash,
                RoleId = pendingSignup.RoleId,
                CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
            };

            await _repository.CreateAsync(user);
            var roleName = await GetRoleNameAsync(user.RoleId);
            await PublishUserCreatedEventAsync(user, roleName);

            _memoryCache.Remove(cacheKey);

            var accessToken = _jwt.GenerateToken(user.UserId, user.Email, roleName);
            var refreshToken = await CreateAndStoreRefreshTokenAsync(user.UserId);
            return MapToAuthDto(user, roleName, accessToken, refreshToken);
        }
        #endregion
        #region RegisterAsync
        /// <summary>
        /// Legacy entry point; registration is completed via OTP verification instead of a direct call.
        /// </summary>
        public async Task<AuthDTO> RegisterAsync(RegisterDTO dto)
        {
            throw new RequestValidationException("Direct signup is disabled. Request OTP and verify OTP to create account.");
        }
        #endregion
        #region LoginAsync
        /// <summary>
        /// Validates credentials, issues JWT and refresh token, and updates last-login metadata.
        /// </summary>
        public async Task<AuthDTO> LoginAsync(LoginDTO dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            var user = await _repository.GetByEmailAsync(normalizedEmail);

            if (user == null)
                throw new NotFoundException("User doesn't exist");

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
                throw new RequestValidationException("Incorrect password");

            var roleName = await GetRoleNameAsync(user.RoleId);
            var accessToken = _jwt.GenerateToken(user.UserId, user.Email, roleName);
            var refreshToken = await CreateAndStoreRefreshTokenAsync(user.UserId);

            return MapToAuthDto(user, roleName, accessToken, refreshToken);
        }
        #endregion
        #region GoogleSignupAsync
        /// <summary>
        /// Verifies the Google ID token, creates or links the user, and returns auth tokens.
        /// </summary>
        public async Task<AuthDTO> GoogleSignupAsync(GoogleSignupDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
                throw new RequestValidationException("Google id token is required");

            var clientId = _googleAuthSettings.ClientId?.Trim();
            if (string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Google OAuth ClientId is not configured");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
            }
            catch (InvalidJwtException)
            {
                throw new RequestValidationException("Invalid Google token");
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
                throw new RequestValidationException("Google account email is not available");

            var normalizedEmail = NormalizeEmail(payload.Email);
            var user = await _repository.GetByEmailAsync(normalizedEmail);

            if (user == null)
            {
                var roleId = await GetDefaultCustomerRoleIdAsync();
                var displayName = string.IsNullOrWhiteSpace(payload.Name)
                    ? normalizedEmail.Split('@')[0]
                    : payload.Name.Trim();

                user = new User
                {
                    Name = displayName,
                    Email = normalizedEmail,
                    Phone = string.Empty,
                    PasswordHash = PasswordHasher.Hash(CreateSecureToken()),
                    RoleId = roleId,
                    CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
                };

                await _repository.CreateAsync(user);
                var createdRoleName = await GetRoleNameAsync(user.RoleId);
                await PublishUserCreatedEventAsync(user, createdRoleName);
            }

            var roleName = await GetRoleNameAsync(user.RoleId);
            var accessToken = _jwt.GenerateToken(user.UserId, user.Email, roleName);
            var refreshToken = await CreateAndStoreRefreshTokenAsync(user.UserId);

            return MapToAuthDto(user, roleName, accessToken, refreshToken);
        }
        #endregion
        #region LogoutAsync
        /// <summary>
        /// Revokes active refresh tokens for the user so refresh-based access stops.
        /// </summary>
        public async Task LogoutAsync(int userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > TimeZoneHelper.GetCurrentUtcTime())
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }
        #endregion
        #region ChangePasswordAsync
        /// <summary>
        /// Verifies current password, updates stored hash, and may invalidate existing refresh tokens.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO dto)
        {
            var user = await GetRequiredUserAsync(userId);

            if (!PasswordHasher.Verify(dto.OldPassword, user.PasswordHash))
                throw new RequestValidationException("Incorrect password");

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            await _repository.UpdateAsync(user);

            return true;
        }
        #endregion
        #region GetProfileAsync
        /// <summary>
        /// Returns profile async.
        /// </summary>
        public async Task<AuthDTO> GetProfileAsync(int userId)
        {
            var user = await GetRequiredUserAsync(userId);
            var roleName = await GetRoleNameAsync(user.RoleId);
            return MapToAuthDto(user, roleName, string.Empty, string.Empty);
        }
        #endregion
        #region UpdateProfileAsync
        /// <summary>
        /// Updates profile async.
        /// </summary>
        public async Task UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var user = await GetRequiredUserAsync(userId);
            user.Name = dto.Name.Trim();
            user.Phone = dto.Phone.Trim();
            await _repository.UpdateAsync(user);
        }
        #endregion



        #region GetUsersAsync
        /// <summary>
        /// Returns users async.
        /// </summary>
        public async Task<PaginatedResponse<AuthDTO>> GetUsersAsync(int pageNumber = 1, int pageSize = 5)
        {
            var users = await _repository.GetAllUsersAsync();
            var roleMap = await _context.Roles.ToDictionaryAsync(r => r.RoleId, r => r.RoleName);

            var userDtos = users.Select(u => MapToAuthDto(
                u,
                roleMap.TryGetValue(u.RoleId, out var roleName) ? roleName : "UNKNOWN",
                string.Empty,
                string.Empty)).ToList();

            var totalCount = userDtos.Count;
            var pagedUsers = userDtos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedUsers.ToPaginatedResponse(pageNumber, pageSize, totalCount);
        }
        #endregion



        #region GetUserByIdAsync
        /// <summary>
        /// Returns user by id async.
        /// </summary>
        public async Task<AuthDTO> GetUserByIdAsync(int id)
        {
            var user = await GetRequiredUserAsync(id);
            var roleName = await GetRoleNameAsync(user.RoleId);
            return MapToAuthDto(user, roleName, string.Empty, string.Empty);
        }
        #endregion



        #region CreateUserAsync
        /// <summary>
        /// Creates user async.
        /// </summary>
        public async Task CreateUserAsync(CreateUserDTO dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            var existingUser = await _repository.GetByEmailAsync(normalizedEmail);
            if (existingUser != null)
                throw new ConflictException("User already exists");

            await EnsureRoleExistsAsync(dto.RoleId);

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = normalizedEmail,
                Phone = dto.Phone.Trim(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                RoleId = dto.RoleId,
                CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
            };

            await _repository.CreateAsync(user);
            await PublishUserCreatedEventAsync(user, await GetRoleNameAsync(user.RoleId));
        }
        #endregion



        #region ResendWelcomeEmailAsync
        /// <summary>
        /// Sends the welcome or onboarding email again for an existing user (admin).
        /// </summary>
        public async Task ResendWelcomeEmailAsync(int id)
        {
            var user = await GetRequiredUserAsync(id);
            var roleName = await GetRoleNameAsync(user.RoleId);
            await PublishUserCreatedEventAsync(user, roleName);
        }
        #endregion



        #region UpdateUserAsync
        /// <summary>
        /// Updates user async.
        /// </summary>
        public async Task UpdateUserAsync(int id, UpdateUserDTO dto)
        {
            var user = await GetRequiredUserAsync(id);
            var normalizedEmail = NormalizeEmail(dto.Email);
            var existingWithEmail = await _repository.GetByEmailAsync(normalizedEmail);

            if (existingWithEmail != null && existingWithEmail.UserId != id)
                throw new ConflictException("Email is already in use");

            user.Name = dto.Name.Trim();
            user.Email = normalizedEmail;
            user.Phone = dto.Phone.Trim();

            await _repository.UpdateAsync(user);
        }
        #endregion



        #region AssignRoleAsync
        /// <summary>
        /// Assigns role async.
        /// </summary>
        public async Task AssignRoleAsync(int id, int roleId)
        {
            var user = await GetRequiredUserAsync(id);
            await EnsureRoleExistsAsync(roleId);

            user.RoleId = roleId;
            await _repository.UpdateAsync(user);
        }
        #endregion



        #region DeleteUserAsync
        /// <summary>
        /// Deletes user async.
        /// </summary>
        public async Task DeleteUserAsync(int id)
        {
            var user = await GetRequiredUserAsync(id);
            await _repository.DeleteAsync(user);
        }
        #endregion



        #region GetRolesAsync
        /// <summary>
        /// Returns roles async.
        /// </summary>
        public async Task<PaginatedResponse<RoleDTO>> GetRolesAsync(int pageNumber = 1, int pageSize = 5)
        {
            var roles = await _context.Roles
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            var totalCount = roles.Count;
            var pagedRoles = roles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedRoles.ToPaginatedResponse(pageNumber, pageSize, totalCount);
        }
        #endregion



        #region CreateRoleAsync
        /// <summary>
        /// Creates role async.
        /// </summary>
        public async Task<RoleDTO> CreateRoleAsync(CreateRoleDTO dto)
        {
            var roleName = dto.RoleName.Trim().ToUpperInvariant();
            var exists = await _context.Roles.AnyAsync(x => x.RoleName == roleName);
            if (exists)
                throw new ConflictException("Role already exists");

            var role = new Role { RoleName = roleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDTO
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }
        #endregion



        #region RefreshTokenAsync
        /// <summary>
        /// Validates refresh token hash, rotates tokens, and returns a new JWT pair.
        /// </summary>
        public async Task<AuthDTO> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new RequestValidationException("Refresh token is required");

            var tokenHash = TokenHasher.Hash(refreshToken);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
            if (token == null || token.IsRevoked || token.ExpiresAt < TimeZoneHelper.GetCurrentUtcTime())
                throw new RequestValidationException("Invalid refresh token");

            var user = await GetRequiredUserAsync(token.UserId);
            token.IsRevoked = true;

            var roleName = await GetRoleNameAsync(user.RoleId);
            var newAccessToken = _jwt.GenerateToken(user.UserId, user.Email, roleName);
            var newRefreshToken = await CreateAndStoreRefreshTokenAsync(user.UserId);

            await _context.SaveChangesAsync();
            return MapToAuthDto(user, roleName, newAccessToken, newRefreshToken);
        }
        #endregion



        #region ForgotPasswordAsync
        /// <summary>
        /// Creates a password-reset token and emails instructions (no user enumeration on success).
        /// </summary>
        public async Task ForgotPasswordAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await _repository.GetByEmailAsync(normalizedEmail);
            if (user == null)
                return;

            var now = TimeZoneHelper.GetCurrentUtcTime();
            var staleTokens = await _context.PasswordResetTokens
                .Where(x => x.IsUsed || x.ExpiresAt < now)
                .ToListAsync();

            if (staleTokens.Count > 0)
            {
                _context.PasswordResetTokens.RemoveRange(staleTokens);
            }

            var existingActiveTokens = await _context.PasswordResetTokens
                .Where(x => x.UserId == user.UserId && !x.IsUsed && x.ExpiresAt >= now)
                .ToListAsync();

            foreach (var activeToken in existingActiveTokens)
            {
                activeToken.IsUsed = true;
            }

            var otp = await CreateUniquePasswordResetOtpAsync();
            var tokenHash = TokenHasher.Hash(otp);

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(10),
                IsUsed = false
            });

            await _context.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(normalizedEmail, otp);
        }
        #endregion



        #region ResetPasswordAsync
        /// <summary>
        /// Applies a new password when the reset token is valid and not expired.
        /// </summary>
        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);
            var user = await _repository.GetByEmailAsync(normalizedEmail);
            if (user == null)
                throw new RequestValidationException("Invalid OTP");

            var tokenHash = TokenHasher.Hash(dto.Token);
            var token = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.UserId == user.UserId && x.TokenHash == tokenHash);

            if (token == null || token.IsUsed || token.ExpiresAt < TimeZoneHelper.GetCurrentUtcTime())
                throw new RequestValidationException("Invalid OTP");

            user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
            token.IsUsed = true;

            await _context.SaveChangesAsync();
        }
        #endregion



        #region GetRequiredUserAsync
        /// <summary>
        /// Returns required user async.
        /// </summary>
        private async Task<User> GetRequiredUserAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found");

            return user;
        }
        #endregion



        #region EnsureRoleExistsAsync
        /// <summary>
        /// Ensures role exists async.
        /// </summary>
        private async Task EnsureRoleExistsAsync(int roleId)
        {
            var exists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!exists)
                throw new NotFoundException("Role not found");
        }
        #endregion



        #region GetRoleNameAsync
        /// <summary>
        /// Returns role name async.
        /// </summary>
        private async Task<string> GetRoleNameAsync(int roleId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if (role == null)
                throw new NotFoundException("Role not found");

            return role.RoleName;
        }
        #endregion



        #region GetDefaultCustomerRoleIdAsync
        /// <summary>
        /// Returns default customer role id async.
        /// </summary>
        private async Task<int> GetDefaultCustomerRoleIdAsync()
        {
            var customerRole = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleName == "CUSTOMER");

            if (customerRole != null)
                return customerRole.RoleId;

            var fallbackRole = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (fallbackRole != null)
                return fallbackRole.RoleId;

            throw new InvalidOperationException("No roles found. Please create a CUSTOMER role first.");
        }
        #endregion



        #region CreateAndStoreRefreshTokenAsync
        /// <summary>
        /// Creates and store refresh token async.
        /// </summary>
        private async Task<string> CreateAndStoreRefreshTokenAsync(int userId)
        {
            var rawToken = CreateSecureToken();
            var tokenHash = TokenHasher.Hash(rawToken);

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = TimeZoneHelper.GetCurrentUtcTime().AddDays(7),
                IsRevoked = false
            });

            await _context.SaveChangesAsync();
            return rawToken;
        }
        #endregion



        #region CreateSecureToken
        /// <summary>
        /// Creates secure token.
        /// </summary>
        private static string CreateSecureToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
        #endregion



        #region CreateUniquePasswordResetOtpAsync
        /// <summary>
        /// Creates unique password reset otp async.
        /// </summary>
        private async Task<string> CreateUniquePasswordResetOtpAsync()
        {
            const int maxAttempts = 10;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
                var otpHash = TokenHasher.Hash(otp);

                var exists = await _context.PasswordResetTokens.AnyAsync(x => x.TokenHash == otpHash && !x.IsUsed && x.ExpiresAt >= TimeZoneHelper.GetCurrentUtcTime());
                if (!exists)
                {
                    return otp;
                }
            }

            throw new InvalidOperationException("Unable to generate password reset OTP. Please try again.");
        }
        #endregion



        #region NormalizeEmail
        /// <summary>
        /// Normalizes email.
        /// </summary>
        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
        #endregion



        #region GetSignupOtpCacheKey
        /// <summary>
        /// Returns signup otp cache key.
        /// </summary>
        private static string GetSignupOtpCacheKey(string normalizedEmail)
        {
            return $"signup-otp:{normalizedEmail}";
        }
        #endregion



        #region CreateOtp
        /// <summary>
        /// Creates otp.
        /// </summary>
        private static string CreateOtp()
        {
            return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        }
        #endregion



        #region PublishUserCreatedEventAsync
        /// <summary>
        /// Publishes user created event async.
        /// </summary>
        private async Task PublishUserCreatedEventAsync(User user, string roleName)
        {
            await _eventPublisher.PublishAsync(
                RabbitMqQueues.UserCreatedQueue,
                new UserCreatedEvent
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = roleName,
                    Timestamp = user.CreatedAt
                });
        }
        #endregion



        #region MapToAuthDto
        /// <summary>
        /// Maps authentication result data to the response DTO.
        /// </summary>
        private static AuthDTO MapToAuthDto(User user, string roleName, string token, string refreshToken)
        {
            return new AuthDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Token = token,
                RefreshToken = refreshToken,
                Role = roleName
            };
        }
        #endregion

        #region Nested types
        /// <summary>
        /// Cached signup draft and OTP verification state before the user row is created.
        /// </summary>
        private sealed class PendingSignupOtp
        {
            public string Name { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string Phone { get; init; } = string.Empty;
            public string PasswordHash { get; init; } = string.Empty;
            public int RoleId { get; init; }
            public string OtpHash { get; init; } = string.Empty;
            public DateTime ExpiresAt { get; init; }
            public int RemainingAttempts { get; set; }
        }
        #endregion
    }
}





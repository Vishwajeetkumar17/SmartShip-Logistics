/// <summary>
/// Provides backend implementation for IUserRepository.
/// </summary>

using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Repositories
{
    /// <summary>
    /// Represents IUserRepository.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync();
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}



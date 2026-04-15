using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Repositories
{
    /// <summary>
    /// Contract for iuser persistence operations.
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



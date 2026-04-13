/// <summary>
/// Provides backend implementation for UserRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.IdentityService.Data;
using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Repositories
{
    /// <summary>
    /// Represents UserRepository.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Executes GetByEmailAsync.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// Executes GetByIdAsync.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Executes GetAllUsersAsync.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Executes CreateAsync.
        /// </summary>
        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Executes UpdateAsync.
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Executes DeleteAsync.
        /// </summary>
        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}



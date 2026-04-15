using Microsoft.EntityFrameworkCore;
using SmartShip.IdentityService.Data;
using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Repositories
{
    /// <summary>
    /// Repository for user data access operations.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        /// <summary>
        /// Registers r repository.
        /// </summary>
        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a user by email address.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        /// <summary>
        /// Returns a record by identifier.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// Returns all users async.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Creates async.
        /// </summary>
        public async Task CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates async.
        /// </summary>
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes async.
        /// </summary>
        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}



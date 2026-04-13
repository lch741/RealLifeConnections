using api.Models;
using backend.Data;
using backend.DTOs;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class UserRepository : IUserInterface
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            return _context.Users.AnyAsync(user => user.Email == email);
        }

        public Task<bool> UserNameExistsAsync(string userName)
        {
            return _context.Users.AnyAsync(user => user.UserName == userName);
        }

        public Task<AppUser?> GetUserByEmailAsync(string email)
        {
            return _context.Users
                .Include(user => user.Interests)
                .ThenInclude(interest => interest.Category)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public Task<List<InterestCategory>> GetInterestCategoriesAsync()
        {
            return _context.InterestCategories
                .OrderBy(category => category.Id)
                .ToListAsync();
        }

        public async Task<AppUser> CreateUserAsync(
            AppUser user,
            IEnumerable<RegisterInterestSelectionDto> interests)
        {
            user.Interests = interests
                .SelectMany(selection => selection.Interests.Select(interest => new UserInterest
                {
                    CategoryId = selection.CategoryId,
                    SubCategory = interest
                }))
                .ToList();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}

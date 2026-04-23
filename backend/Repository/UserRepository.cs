using api.Models;
using backend.Data;
using backend.DTO.Matching;
using backend.DTOs;
using backend.Helper;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class UserRepository : IUserRepository
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
                .Include(user => user.Verifications)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public Task<AppUser?> GetUserByIdAsync(int userId)
        {
            return _context.Users
                .Include(user => user.Interests)
                .ThenInclude(interest => interest.Category)
                .Include(user => user.Verifications)
                .FirstOrDefaultAsync(user => user.Id == userId);
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

        public async Task<AppUser> UpdateProfileAsync(AppUser user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName.Trim();
            }

            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            user.City = string.IsNullOrWhiteSpace(dto.City) ? "online" : dto.City.Trim();

            var currentInterests = await _context.UserInterests
                .Where(interest => interest.UserId == user.Id)
                .ToListAsync();

            _context.UserInterests.RemoveRange(currentInterests);

            var updatedInterests = dto.InterestSelections
                .SelectMany(selection => selection.Interests.Select(interest => new UserInterest
                {
                    UserId = user.Id,
                    CategoryId = selection.CategoryId,
                    SubCategory = interest
                }))
                .ToList();

            _context.UserInterests.AddRange(updatedInterests);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(user.Id) ?? user;
        }

        public async Task<AppUser> SaveAvatarUrlAsync(AppUser user, string avatarUrl)
        {
            user.ProfileImageUrl = avatarUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return await GetUserByIdAsync(user.Id) ?? user;
        }

        public async Task<Verification> AddVerificationAsync(Verification verification)
        {
            _context.Verifications.Add(verification);
            await _context.SaveChangesAsync();
            return verification;
        }

        public Task<List<AppUser>> GetVerifiedMatchCandidatesAsync(int userId)
        {
            return _context.Users
                .Include(user => user.Interests)
                .ThenInclude(interest => interest.Category)
                .Include(user => user.Verifications)
                .Where(user => user.Id != userId && user.IsVerified)
                .OrderBy(user => user.UserName)
                .ToListAsync();
        }

        public async Task<List<AppUser>> SearchCandidatesAsync(int userId, UserQueryObject queryObject)
        {
            var pageNumber = queryObject.PageNumber < 1 ? 1 : queryObject.PageNumber;
            var pageSize = queryObject.PageSize <= 0 ? 20 : queryObject.PageSize;

            var candidatesQuery = _context.Users
                .AsNoTracking()
                .Include(user => user.Interests)
                .ThenInclude(interest => interest.Category)
                .Where(user => user.IsVerified && user.Id != userId);

            if (!string.IsNullOrWhiteSpace(queryObject.UserName))
            {
                var userName = queryObject.UserName.Trim();
                candidatesQuery = candidatesQuery.Where(user =>
                    EF.Functions.ILike(user.UserName, $"%{userName}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryObject.City))
            {
                var city = queryObject.City.Trim();
                candidatesQuery = candidatesQuery.Where(user =>
                    EF.Functions.ILike(user.City, $"%{city}%"));
            }

            if (!string.IsNullOrWhiteSpace(queryObject.Interest))
            {
                var interest = queryObject.Interest.Trim();
                candidatesQuery = candidatesQuery.Where(user =>
                    user.Interests.Any(userInterest =>
                        EF.Functions.ILike(userInterest.SubCategory, $"%{interest}%") ||
                        EF.Functions.ILike(userInterest.Category.Name, $"%{interest}%")));
            }

            return await candidatesQuery
                .OrderBy(user => user.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

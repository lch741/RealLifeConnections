using backend.Models;
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
            user.Suburb = string.IsNullOrWhiteSpace(dto.City) ? "online" : dto.City.Trim();
            user.Gender = Enum.TryParse<Gender>(dto.Gender, true, out var gender)
                ? gender
                : user.Gender;
            user.Age = dto.Age;
            user.Culture = string.IsNullOrWhiteSpace(dto.Culture) ? null : dto.Culture.Trim();

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

            var candidatesQuery = ApplyBaseSearchScope(_context.Users.AsNoTracking(), userId);

            candidatesQuery = ApplyUserNameFilter(candidatesQuery, queryObject.UserName);
            candidatesQuery = ApplyCityFilter(candidatesQuery, queryObject.City);
            candidatesQuery = ApplyInterestFilter(candidatesQuery, queryObject.Interest);
            candidatesQuery = ApplyGenderFilter(candidatesQuery, queryObject.Gender);
            candidatesQuery = ApplyAgeFilter(candidatesQuery, queryObject.Age);
            candidatesQuery = ApplyCultureFilter(candidatesQuery, queryObject.Culture);

            return await candidatesQuery
                .OrderBy(user => user.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private static IQueryable<AppUser> ApplyBaseSearchScope(
            IQueryable<AppUser> query,
            int userId)
        {
            return query
                .Include(user => user.Interests)
                .ThenInclude(interest => interest.Category)
                .Where(user => user.IsVerified && user.Id != userId);
        }

        private static IQueryable<AppUser> ApplyUserNameFilter(
            IQueryable<AppUser> query,
            string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return query;
            }

            var normalizedUserName = userName.Trim();
            return query.Where(user => EF.Functions.ILike(user.UserName, $"%{normalizedUserName}%"));
        }

        private static IQueryable<AppUser> ApplyCityFilter(
            IQueryable<AppUser> query,
            string? city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return query;
            }

            var normalizedCity = city.Trim();
            return query.Where(user => EF.Functions.ILike(user.Suburb, $"%{normalizedCity}%") || EF.Functions.ILike(user.Region, $"%{normalizedCity}%"));
        }

        private static IQueryable<AppUser> ApplyInterestFilter(
            IQueryable<AppUser> query,
            string? interest)
        {
            if (string.IsNullOrWhiteSpace(interest))
            {
                return query;
            }

            var normalizedInterest = interest.Trim();
            return query.Where(user =>
                user.Interests.Any(userInterest =>
                    EF.Functions.ILike(userInterest.SubCategory, $"%{normalizedInterest}%") ||
                    EF.Functions.ILike(userInterest.Category.Name, $"%{normalizedInterest}%")));
        }

        private static IQueryable<AppUser> ApplyGenderFilter(
            IQueryable<AppUser> query,
            string? genderText)
        {
            if (string.IsNullOrWhiteSpace(genderText))
            {
                return query;
            }

            return Enum.TryParse<Gender>(genderText.Trim(), true, out var genderFilter)
                ? query.Where(user => user.Gender == genderFilter)
                : query;
        }

        private static IQueryable<AppUser> ApplyAgeFilter(
            IQueryable<AppUser> query,
            int? age)
        {
            if (!age.HasValue)
            {
                return query;
            }

            if (age.Value <= 0)
            {
                throw new InvalidOperationException("Age must be greater than zero.");
            }

            var ageStart = (age.Value / 10) * 10;
            var ageEnd = ageStart + 10;
            return query.Where(user =>
                user.Age.HasValue &&
                user.Age.Value >= ageStart &&
                user.Age.Value < ageEnd);
        }

        private static IQueryable<AppUser> ApplyCultureFilter(
            IQueryable<AppUser> query,
            string? culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                return query;
            }

            var normalizedCulture = culture.Trim();
            return query.Where(user =>
                user.Culture != null && EF.Functions.ILike(user.Culture, $"%{normalizedCulture}%"));
        }
    }
}

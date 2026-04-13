using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Models;
using backend.DTOs;
using backend.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace backend.Service
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserInterface _userRepository;

        public UserService(IConfiguration configuration, IUserInterface userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var normalizedUserName = dto.UserName.Trim();

            if (await _userRepository.EmailExistsAsync(normalizedEmail))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            if (await _userRepository.UserNameExistsAsync(normalizedUserName))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            var categories = await _userRepository.GetInterestCategoriesAsync();
            ValidateSelections(dto.InterestSelections, categories);

            var user = new AppUser
            {
                Email = normalizedEmail,
                UserName = normalizedUserName,
                PasswordHash = HashPassword(dto.Password),
                Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim(),
                ProfileImageUrl = string.IsNullOrWhiteSpace(dto.ProfileImageUrl) ? null : dto.ProfileImageUrl.Trim()
            };

            var createdUser = await _userRepository.CreateUserAsync(user, dto.InterestSelections);
            return BuildAuthResponse(createdUser, categories, "Registration successful.");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetUserByEmailAsync(normalizedEmail);

            if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var categories = await _userRepository.GetInterestCategoriesAsync();
            return BuildAuthResponse(user, categories, "Login successful.");
        }

        private AuthResponseDto BuildAuthResponse(
            AppUser user,
            List<InterestCategory> categories,
            string message)
        {
            return new AuthResponseDto
            {
                Message = message,
                Token = GenerateJwtToken(user),
                User = new AuthUserDto
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    Bio = user.Bio,
                    ProfileImageUrl = user.ProfileImageUrl,
                    InterestSelections = user.Interests
                        .GroupBy(interest => interest.CategoryId)
                        .OrderBy(group => group.Key)
                        .Select(group => new RegisterInterestResultDto
                        {
                            CategoryId = group.Key,
                            CategoryName = categories.First(category => category.Id == group.Key).Name,
                            Interests = group
                                .Select(interest => interest.SubCategory)
                                .ToList()
                        })
                        .ToList()
                }
            };
        }

        private static void ValidateSelections(
            List<RegisterInterestSelectionDto> selections,
            List<InterestCategory> categories)
        {
            if (selections.Count == 0)
            {
                throw new InvalidOperationException("At least one interest category must be selected.");
            }

            var validCategoryIds = categories.Select(category => category.Id).ToHashSet();
            var duplicateCategoryIds = selections
                .GroupBy(selection => selection.CategoryId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateCategoryIds.Count > 0)
            {
                throw new InvalidOperationException("Each category can only be selected once.");
            }

            foreach (var selection in selections)
            {
                if (!validCategoryIds.Contains(selection.CategoryId))
                {
                    throw new InvalidOperationException($"Category {selection.CategoryId} is invalid. Use the 8 seeded InterestCategory options only.");
                }

                var cleanedInterests = selection.Interests
                    .Select(interest => interest.Trim())
                    .Where(interest => !string.IsNullOrWhiteSpace(interest))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (cleanedInterests.Count == 0)
                {
                    throw new InvalidOperationException($"Category {selection.CategoryId} must include at least one interest.");
                }

                if (cleanedInterests.Count > 3)
                {
                    throw new InvalidOperationException($"Category {selection.CategoryId} can contain at most 3 interests.");
                }

                selection.Interests = cleanedInterests;
            }
        }

        private string GenerateJwtToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("username", user.UserName)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}

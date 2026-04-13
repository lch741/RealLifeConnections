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
        private readonly IAzureFaceService _azureFaceService;
        private readonly IUserInterface _userRepository;

        public UserService(
            IConfiguration configuration,
            IAzureFaceService azureFaceService,
            IUserInterface userRepository)
        {
            _configuration = configuration;
            _azureFaceService = azureFaceService;
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

        public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal)
        {
            var user = await GetCurrentUserAsync(principal);
            var categories = await _userRepository.GetInterestCategoriesAsync();
            return MapProfile(user, categories);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto)
        {
            var user = await GetCurrentUserAsync(principal);
            var categories = await _userRepository.GetInterestCategoriesAsync();

            if (!string.IsNullOrWhiteSpace(dto.UserName) &&
                !string.Equals(dto.UserName.Trim(), user.UserName, StringComparison.OrdinalIgnoreCase) &&
                await _userRepository.UserNameExistsAsync(dto.UserName.Trim()))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            ValidateSelections(dto.InterestSelections, categories);

            var updatedUser = await _userRepository.UpdateProfileAsync(user, dto);
            return MapProfile(updatedUser, categories);
        }

        public async Task<UserProfileDto> SaveAvatarAsync(ClaimsPrincipal principal, SaveAvatarDto dto)
        {
            var user = await GetCurrentUserAsync(principal);
            var categories = await _userRepository.GetInterestCategoriesAsync();
            user.IsVerified = false;
            var updatedUser = await _userRepository.SaveAvatarUrlAsync(user, dto.AvatarUrl.Trim());
            await _userRepository.AddVerificationAsync(new Verification
            {
                UserId = updatedUser.Id,
                ImageUrl = dto.AvatarUrl.Trim(),
                Status = "pending"
            });

            updatedUser = await _userRepository.GetUserByIdAsync(updatedUser.Id) ?? updatedUser;
            return MapProfile(updatedUser, categories);
        }

        public async Task<FaceVerificationResponseDto> VerifyFaceAsync(ClaimsPrincipal principal, FaceVerificationRequestDto dto)
        {
            var user = await GetCurrentUserAsync(principal);

            if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
            {
                throw new InvalidOperationException("Avatar must be uploaded before face verification.");
            }

            var azureResult = await _azureFaceService.VerifyFacesAsync(user.ProfileImageUrl, dto.LiveCaptureUrl.Trim());
            var status = azureResult.IsIdentical ? "approved" : "rejected";

            user.IsVerified = azureResult.IsIdentical;
            await _userRepository.AddVerificationAsync(new Verification
            {
                UserId = user.Id,
                ImageUrl = dto.LiveCaptureUrl.Trim(),
                Status = status
            });

            await _userRepository.SaveAvatarUrlAsync(user, user.ProfileImageUrl);

            return new FaceVerificationResponseDto
            {
                Status = status,
                Message = azureResult.IsIdentical
                    ? "Face verification passed. Matching is now unlocked."
                    : "Face verification failed. Matching remains locked.",
                IsVerified = azureResult.IsIdentical,
                Confidence = azureResult.Confidence
            };
        }

        public async Task<List<MatchCandidateDto>> GetMatchesAsync(ClaimsPrincipal principal)
        {
            var user = await GetCurrentUserAsync(principal);
            if (!user.IsVerified)
            {
                throw new InvalidOperationException("Face verification is required before matching.");
            }

            var categories = await _userRepository.GetInterestCategoriesAsync();
            var candidates = await _userRepository.GetVerifiedMatchCandidatesAsync(user.Id);
            var currentInterests = user.Interests
                .GroupBy(interest => interest.CategoryId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(interest => interest.SubCategory).ToHashSet(StringComparer.OrdinalIgnoreCase));

            return candidates
                .Select(candidate =>
                {
                    var sharedGroups = candidate.Interests
                        .Where(interest => currentInterests.ContainsKey(interest.CategoryId)
                            && currentInterests[interest.CategoryId].Contains(interest.SubCategory))
                        .GroupBy(interest => interest.CategoryId)
                        .Select(group => new RegisterInterestResultDto
                        {
                            CategoryId = group.Key,
                            CategoryName = categories.First(category => category.Id == group.Key).Name,
                            Interests = group.Select(interest => interest.SubCategory).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                        })
                        .ToList();

                    return new MatchCandidateDto
                    {
                        UserName = candidate.UserName,
                        Bio = candidate.Bio,
                        AvatarUrl = candidate.ProfileImageUrl,
                        SharedInterests = sharedGroups
                    };
                })
                .Where(candidate => candidate.SharedInterests.Count > 0)
                .OrderByDescending(candidate => candidate.SharedInterests.Sum(group => group.Interests.Count))
                .ToList();
        }

        private async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("User identity is missing.");

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("User identity is invalid.");
            }

            return await _userRepository.GetUserByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");
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
                    InterestSelections = GroupInterests(user.Interests, categories)
                }
            };
        }

        private UserProfileDto MapProfile(AppUser user, List<InterestCategory> categories)
        {
            var latestStatus = user.Verifications
                .OrderByDescending(verification => verification.CreatedAt)
                .Select(verification => verification.Status)
                .FirstOrDefault() ?? "pending";

            return new UserProfileDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Bio = user.Bio,
                AvatarUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                VerificationStatus = latestStatus,
                CanMatch = user.IsVerified,
                InterestSelections = GroupInterests(user.Interests, categories)
            };
        }

        private static List<RegisterInterestResultDto> GroupInterests(
            IEnumerable<UserInterest> interests,
            List<InterestCategory> categories)
        {
            return interests
                .GroupBy(interest => interest.CategoryId)
                .OrderBy(group => group.Key)
                .Select(group => new RegisterInterestResultDto
                {
                    CategoryId = group.Key,
                    CategoryName = categories.First(category => category.Id == group.Key).Name,
                    Interests = group.Select(interest => interest.SubCategory).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                })
                .ToList();
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
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
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

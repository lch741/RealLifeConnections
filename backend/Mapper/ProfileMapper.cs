using backend.DTOs;
using api.Models;

namespace backend.Mapper
{
    public static class ProfileMapper
    {
        public static UserProfileDto ToUserProfile(AppUser user, List<InterestCategory> categories)
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
                City = user.City,
                AvatarUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                VerificationStatus = latestStatus,
                CanMatch = user.IsVerified,
                Gender = user.Gender.ToString(),
                Age = user.Age,
                Culture = user.Culture,
                InterestSelections = InterestMapper.ToInterestResults(user.Interests, categories)
            };
        }

        public static AppUser ApplyProfileUpdate(AppUser user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName.Trim();
            }

            user.City = dto.City;
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Gender))
            {
                if (Enum.TryParse<Gender>(dto.Gender, true, out var parsedGender))
                {
                    user.Gender = parsedGender;
                }
                else
                {
                    user.Gender = Gender.NotToTell;
                }
            }

            if (dto.Age.HasValue)
            {
                user.Age = dto.Age;
            }

            user.Culture = string.IsNullOrWhiteSpace(dto.Culture) ? null : dto.Culture.Trim();
            return user;
        }
    }
}
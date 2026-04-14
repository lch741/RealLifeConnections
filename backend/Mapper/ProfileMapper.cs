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
                AvatarUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                VerificationStatus = latestStatus,
                CanMatch = user.IsVerified,
                InterestSelections = InterestMapper.ToInterestResults(user.Interests, categories)
            };
        }

        public static AppUser ApplyProfileUpdate(AppUser user, UpdateProfileDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName.Trim();
            }

            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            return user;
        }
    }
}
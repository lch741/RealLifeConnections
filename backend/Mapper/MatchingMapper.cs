using backend.DTOs;
using backend.Models;
using backend.DTO.Matching;

namespace backend.Mapper
{
    public static class MatchingMapper
    {
        public static MatchCandidateDto ToMatchCandidate(
            AppUser candidate,
            HashSet<string> currentInterests,
            List<InterestCategory> categories,
            int compatibilityScore = 0)
        {
            return new MatchCandidateDto
            {
                UserId = candidate.Id,
                UserName = candidate.UserName,
                Bio = candidate.Bio,
                Region = candidate.Region,
                Suburb = candidate.Suburb,
                AvatarUrl = candidate.ProfileImageUrl,
                Gender = candidate.Gender.ToString(),
                Age = candidate.Age,
                Culture = candidate.Culture,
                CompatibilityScore = compatibilityScore,
                Personality = ProfileMapper.ToPersonalityDto(candidate),
                SharedInterests = InterestMapper.ToSharedInterestResults(candidate.Interests, currentInterests, categories)
            };
        }

        public static SearchingCandidateDto ToSearchingCandidate(
            AppUser candidate,
            List<InterestCategory> categories,
            int compatibilityScore = 0)
        {
            return new SearchingCandidateDto
            {
                UserId = candidate.Id,
                UserName = candidate.UserName,
                Bio = candidate.Bio,
                Region = candidate.Region,
                Suburb = candidate.Suburb,
                AvatarUrl = candidate.ProfileImageUrl,
                Gender = candidate.Gender.ToString(),
                Age = candidate.Age,
                Culture = candidate.Culture,
                CompatibilityScore = compatibilityScore,
                Personality = ProfileMapper.ToPersonalityDto(candidate),
                Interests = InterestMapper.ToInterestResults(candidate.Interests, categories)
            };
        }

        public static UserMatchDto ToUserMatch(
            AppUser user,
            List<InterestCategory> categories,
            int compatibilityScore,
            string? matchReason = null)
        {
            return new UserMatchDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Bio = user.Bio,
                Region = user.Region,
                Suburb = user.Suburb,
                AvatarUrl = user.ProfileImageUrl,
                Gender = user.Gender.ToString(),
                Age = user.Age,
                Culture = user.Culture,
                CompatibilityScore = compatibilityScore,
                MatchReason = matchReason,
                Personality = ProfileMapper.ToPersonalityDto(user),
                SharedInterests = InterestMapper.ToInterestResults(user.Interests, categories)
            };
        }
    }
}
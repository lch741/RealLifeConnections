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
            List<InterestCategory> categories)
        {
            return new MatchCandidateDto
            {
                UserName = candidate.UserName,
                Bio = candidate.Bio,
                City = candidate.Suburb ?? candidate.Region,
                AvatarUrl = candidate.ProfileImageUrl,
                Gender = candidate.Gender.ToString(),
                Age = candidate.Age,
                Culture = candidate.Culture,
                SharedInterests = InterestMapper.ToSharedInterestResults(candidate.Interests, currentInterests, categories)
            };
        }

        public static SearchingCandidateDto ToSearchingCandidate(
            AppUser candidate,
            List<InterestCategory> categories)
        {
            return new SearchingCandidateDto
            {
                UserName = candidate.UserName,
                Bio = candidate.Bio,
                City = candidate.Suburb ?? candidate.Region,
                AvatarUrl = candidate.ProfileImageUrl,
                Gender = candidate.Gender.ToString(),
                Age = candidate.Age,
                Culture = candidate.Culture,
                Interests = InterestMapper.ToInterestResults(candidate.Interests, categories)
            };
        }
    }
}
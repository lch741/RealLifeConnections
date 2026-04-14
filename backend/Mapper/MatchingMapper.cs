using backend.DTOs;
using api.Models;

namespace backend.Mapper
{
    public static class MatchingMapper
    {
        public static MatchCandidateDto ToMatchCandidate(
            AppUser candidate,
            IReadOnlyDictionary<int, HashSet<string>> currentInterests,
            List<InterestCategory> categories)
        {
            return new MatchCandidateDto
            {
                UserName = candidate.UserName,
                Bio = candidate.Bio,
                AvatarUrl = candidate.ProfileImageUrl,
                SharedInterests = InterestMapper.ToSharedInterestResults(candidate.Interests, currentInterests, categories)
            };
        }
    }
}
using backend.DTOs;
using api.Models;

namespace backend.Mapper
{
    public static class InterestMapper
    {
        public static List<RegisterInterestResultDto> ToInterestResults(
            IEnumerable<UserInterest> interests,
            List<InterestCategory> categories)
        {
            var categoryNameById = categories.ToDictionary(category => category.Id, category => category.Name);

            return interests
                .GroupBy(interest => interest.CategoryId)
                .OrderBy(group => group.Key)
                .Select(group => new RegisterInterestResultDto
                {
                    CategoryId = group.Key,
                    CategoryName = categoryNameById.TryGetValue(group.Key, out var categoryName)
                        ? categoryName
                        : $"Category {group.Key}",
                    Interests = group
                        .Select(interest => interest.SubCategory)
                        .Where(interest => !string.IsNullOrWhiteSpace(interest))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(interest => interest, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .ToList();
        }

        public static List<RegisterInterestResultDto> ToSharedInterestResults(
            IEnumerable<UserInterest> candidateInterests,
            IReadOnlyDictionary<int, HashSet<string>> currentInterests,
            List<InterestCategory> categories)
        {
            var categoryNameById = categories.ToDictionary(category => category.Id, category => category.Name);

            return candidateInterests
                .Where(interest =>
                    currentInterests.TryGetValue(interest.CategoryId, out var myInterests)
                    && myInterests.Contains(interest.SubCategory))
                .GroupBy(interest => interest.CategoryId)
                .OrderBy(group => group.Key)
                .Select(group => new RegisterInterestResultDto
                {
                    CategoryId = group.Key,
                    CategoryName = categoryNameById.TryGetValue(group.Key, out var categoryName)
                        ? categoryName
                        : $"Category {group.Key}",
                    Interests = group
                        .Select(interest => interest.SubCategory)
                        .Where(interest => !string.IsNullOrWhiteSpace(interest))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(interest => interest, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                })
                .ToList();
        }
    }
}
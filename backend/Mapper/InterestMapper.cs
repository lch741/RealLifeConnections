using backend.DTOs;
using backend.Models;
using backend.Helper;
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
            HashSet<string> currentInterests,
            List<InterestCategory> categories)
        {
            var categoryNameById = categories.ToDictionary(category => category.Id, category => category.Name);

            var matchedInterests = candidateInterests
                .Where(interest => currentInterests.Any(my =>
                    MatchHelper.IsRoughMatch(my, interest.SubCategory)));

            return matchedInterests
                .GroupBy(interest => interest.CategoryId)
                .OrderBy(group => group.Key)
                .Select(group => BuildInterestResult(group.Key, group, categoryNameById))
                .ToList();
        }

        private static RegisterInterestResultDto BuildInterestResult(
            int categoryId,
            IEnumerable<UserInterest> interests,
            IReadOnlyDictionary<int, string> categoryNameById)
        {
            return new RegisterInterestResultDto
            {
                CategoryId = categoryId,
                CategoryName = categoryNameById.TryGetValue(categoryId, out var categoryName)
                    ? categoryName
                    : $"Category {categoryId}",
                Interests = interests
                    .Select(interest => interest.SubCategory)
                    .Where(interest => !string.IsNullOrWhiteSpace(interest))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(interest => interest, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }
    }
}
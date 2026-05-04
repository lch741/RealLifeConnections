using backend.DTO.Matching;
using backend.Models;

namespace backend.Mapper
{
    public static class PersonalityMapper
    {
        private const string ChillLabel = "Chill";
        private const string BalancedLabel = "Balanced";
        private const string EnergeticLabel = "Energetic";
        private const string TalkativeLabel = "Talkative";
        private const string QuietLabel = "Quiet";
        private const string PlannerLabel = "Planner";
        private const string SpontaneousLabel = "Spontaneous";
        private const string IntrovertLabel = "Introvert";
        private const string ExtrovertLabel = "Extrovert";
        private const string AnytimePreference = "Anytime";
        public static PersonalityTraitsSummaryDto ToPersonalityTraitsSummary(AppUser user)
        {
            return new PersonalityTraitsSummaryDto
            {
                UserId = user.Id,
                ChillToEnergeticLabel = GetTraitLabel(user.ChillToEnergetic, new[] { ChillLabel, BalancedLabel, EnergeticLabel }),
                ChillToEnergeticScore = user.ChillToEnergetic,
                TalkativeToQuietLabel = GetTraitLabel(user.TalkativeToQuiet, new[] { TalkativeLabel, BalancedLabel, QuietLabel }),
                TalkativeToQuietScore = user.TalkativeToQuiet,
                PlannerToSpontaneousLabel = GetTraitLabel(user.PlannerToSpontaneous, new[] { PlannerLabel, BalancedLabel, SpontaneousLabel }),
                PlannerToSpontaneousScore = user.PlannerToSpontaneous,
                IntrovertToExtrovertLabel = GetTraitLabel(user.IntrovertToExtrovert, new[] { IntrovertLabel, BalancedLabel, ExtrovertLabel }),
                IntrovertToExtrovertScore = user.IntrovertToExtrovert,
                PreferredDaysOfWeek = user.PreferredDaysOfWeek,
                PreferredTimeOfDay = user.PreferredTimeOfDay,
                Summary = BuildPersonalitySummary(user)
            };
        }

        /// <summary>
        /// Get personality compatibility between two users (0-100 scale).
        /// Lower values indicate higher compatibility (opposite ends repel).
        /// </summary>
        public static PersonalityCompatibilityDto GetPersonalityCompatibility(AppUser user1, AppUser user2, double distanceKm = 0)
        {
            var chillScore = CalculateTraitCompatibility(user1.ChillToEnergetic, user2.ChillToEnergetic);
            var talkativeScore = CalculateTraitCompatibility(user1.TalkativeToQuiet, user2.TalkativeToQuiet);
            var plannerScore = CalculateTraitCompatibility(user1.PlannerToSpontaneous, user2.PlannerToSpontaneous);
            var introvertScore = CalculateTraitCompatibility(user1.IntrovertToExtrovert, user2.IntrovertToExtrovert);

            var overallScore = (chillScore + talkativeScore + plannerScore + introvertScore) / 4.0;
            var timeCompatible = CheckTimePreferenceCompatibility(user1, user2);

            return new PersonalityCompatibilityDto
            {
                OverallScore = (int)Math.Round(overallScore),
                ChillToEnergeticScore = chillScore,
                TalkativeToQuietScore = talkativeScore,
                PlannerToSpontaneousScore = plannerScore,
                IntrovertToExtrovertScore = introvertScore,
                TimePreferenceCompatible = timeCompatible,
                DistanceKm = distanceKm > 0 ? distanceKm : null,
                SharedInterestsCount = 0 // To be calculated separately with interest data
            };
        }

        /// <summary>
        /// Calculate trait compatibility between two scores (0-100 scale).
        /// 100 = identical traits, 0 = opposite traits.
        /// </summary>
        private static int CalculateTraitCompatibility(int? trait1, int? trait2)
        {
            if (!trait1.HasValue || !trait2.HasValue)
                return 100; // If unknown, assume compatible

            // Compatibility is inverse of difference
            var difference = Math.Abs(trait1.Value - trait2.Value);
            return 100 - difference;
        }

        /// <summary>
        /// Check if time preferences are compatible.
        /// </summary>
        private static bool CheckTimePreferenceCompatibility(AppUser user1, AppUser user2)
        {
            // If either is "Anytime", they're compatible
            if (user1.PreferredDaysOfWeek == AnytimePreference || user2.PreferredDaysOfWeek == AnytimePreference)
            {
                return true;
            }

            if (user1.PreferredTimeOfDay == AnytimePreference || user2.PreferredTimeOfDay == AnytimePreference)
            {
                return true;
            }

            // Check exact matches
            if (user1.PreferredDaysOfWeek == user2.PreferredDaysOfWeek &&
                user1.PreferredTimeOfDay == user2.PreferredTimeOfDay)
            {
                return true;
            }

            return false;
        }

        private static string? GetTraitLabel(int? score, string[] labels)
        {
            if (!score.HasValue || labels.Length != 3)
                return null;

            if (score.Value < 33)
                return labels[0];
            else if (score.Value < 67)
                return labels[1];
            else
                return labels[2];
        }

        /// <summary>
        /// Build a brief personality summary description.
        /// </summary>
        private static string? BuildPersonalitySummary(AppUser user)
        {
            var traits = new List<string>();

            if (user.ChillToEnergetic.HasValue)
                traits.Add(GetTraitLabel(user.ChillToEnergetic, new[] { ChillLabel, BalancedLabel, EnergeticLabel }) ?? "");

            if (user.TalkativeToQuiet.HasValue)
                traits.Add(GetTraitLabel(user.TalkativeToQuiet, new[] { TalkativeLabel, BalancedLabel, QuietLabel }) ?? "");

            if (user.IntrovertToExtrovert.HasValue)
                traits.Add(GetTraitLabel(user.IntrovertToExtrovert, new[] { IntrovertLabel, BalancedLabel, ExtrovertLabel }) ?? "");

            if (traits.Count == 0)
                return null;

            return $"{string.Join(", ", traits.Where(t => !string.IsNullOrWhiteSpace(t)))}.";
        }

        /// <summary>
        /// Convert MeetupMatchDto with personality compatibility score.
        /// </summary>
        public static MeetupMatchDto ToMeetupMatchDto(
            MeetupEvent meetup,
            int matchScore,
            int timeMatchScore,
            double? distanceKm = null)
        {
            return new MeetupMatchDto
            {
                MeetupId = meetup.Id,
                Title = meetup.Title,
                Description = meetup.Description,
                Region = meetup.Region,
                Suburb = meetup.Suburb,
                LocationName = meetup.LocationName,
                ActivityName = meetup.Activity?.Name ?? "Unknown",
                EventDate = meetup.EventDate,
                StartTime = meetup.StartTime,
                EndTime = meetup.EndTime,
                CurrentParticipants = meetup.Participants.Count,
                MaxParticipants = meetup.MaxParticipants,
                Status = meetup.Status.ToString(),
                MatchScore = matchScore,
                CreatorId = meetup.CreatorId,
                CreatorName = meetup.Creator?.UserName ?? "Unknown",
                DistanceKm = distanceKm,
                TimeMatchScore = timeMatchScore
            };
        }
    }
}

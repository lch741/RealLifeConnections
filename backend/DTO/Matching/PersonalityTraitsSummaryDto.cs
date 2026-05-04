namespace backend.DTO.Matching
{
    /// <summary>
    /// DTO for personality traits summary with labeled values.
    /// </summary>
    public class PersonalityTraitsSummaryDto
    {
        public int UserId { get; set; }

        /// <summary>
        /// Chill/Energetic label (e.g., "Balanced", "Energetic", "Chill").
        /// </summary>
        public string? ChillToEnergeticLabel { get; set; }
        public int? ChillToEnergeticScore { get; set; }

        /// <summary>
        /// Talkative/Quiet label.
        /// </summary>
        public string? TalkativeToQuietLabel { get; set; }
        public int? TalkativeToQuietScore { get; set; }

        /// <summary>
        /// Planner/Spontaneous label.
        /// </summary>
        public string? PlannerToSpontaneousLabel { get; set; }
        public int? PlannerToSpontaneousScore { get; set; }

        /// <summary>
        /// Introvert/Extrovert label.
        /// </summary>
        public string? IntrovertToExtrovertLabel { get; set; }
        public int? IntrovertToExtrovertScore { get; set; }

        public string? PreferredDaysOfWeek { get; set; }
        public string? PreferredTimeOfDay { get; set; }

        /// <summary>
        /// Personality description summary.
        /// </summary>
        public string? Summary { get; set; }
    }
}

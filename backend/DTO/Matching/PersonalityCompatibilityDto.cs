namespace backend.DTO.Matching
{
    /// <summary>
    /// DTO for personality compatibility details between two users.
    /// </summary>
    public class PersonalityCompatibilityDto
    {
        /// <summary>
        /// Overall compatibility score (0-100).
        /// </summary>
        public int OverallScore { get; set; }

        /// <summary>
        /// Chill/Energetic compatibility score (0-100).
        /// </summary>
        public int ChillToEnergeticScore { get; set; }

        /// <summary>
        /// Talkative/Quiet compatibility score (0-100).
        /// </summary>
        public int TalkativeToQuietScore { get; set; }

        /// <summary>
        /// Planner/Spontaneous compatibility score (0-100).
        /// </summary>
        public int PlannerToSpontaneousScore { get; set; }

        /// <summary>
        /// Introvert/Extrovert compatibility score (0-100).
        /// </summary>
        public int IntrovertToExtrovertScore { get; set; }

        /// <summary>
        /// Time preference compatibility (true if compatible).
        /// </summary>
        public bool TimePreferenceCompatible { get; set; }

        /// <summary>
        /// Location proximity in kilometers.
        /// </summary>
        public double? DistanceKm { get; set; }

        /// <summary>
        /// Shared interests count.
        /// </summary>
        public int SharedInterestsCount { get; set; }
    }
}

namespace backend.DTO.Matching
{
    /// <summary>
    /// DTO for personality traits data.
    /// </summary>
    public class PersonalityDto
    {
        /// <summary>
        /// Chill (0) to Energetic (100) scale.
        /// </summary>
        public int? ChillToEnergetic { get; set; }

        /// <summary>
        /// Talkative (0) to Quiet (100) scale.
        /// </summary>
        public int? TalkativeToQuiet { get; set; }

        /// <summary>
        /// Planner (0) to Spontaneous (100) scale.
        /// </summary>
        public int? PlannerToSpontaneous { get; set; }

        /// <summary>
        /// Introvert (0) to Extrovert (100) scale.
        /// </summary>
        public int? IntrovertToExtrovert { get; set; }

        /// <summary>
        /// Preferred days: "Weekday", "Weekend", "Anytime"
        /// </summary>
        public string? PreferredDaysOfWeek { get; set; }

        /// <summary>
        /// Preferred times: "Morning", "Afternoon", "Evening", "Night", "Anytime"
        /// </summary>
        public string? PreferredTimeOfDay { get; set; }

        /// <summary>
        /// Preferred distance for meetups in kilometers.
        /// </summary>
        public int? PreferredDistanceKm { get; set; }
    }
}

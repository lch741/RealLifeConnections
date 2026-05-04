namespace backend.DTO.Matching
{
    /// <summary>
    /// DTO for meetup event recommendations.
    /// </summary>
    public class MeetupMatchDto
    {
        public int MeetupId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string Region { get; set; }
        public required string Suburb { get; set; }
        public string? LocationName { get; set; }

        /// <summary>
        /// Activity name and category.
        /// </summary>
        public required string ActivityName { get; set; }

        /// <summary>
        /// Event date and time.
        /// </summary>
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Current participant count and max capacity.
        /// </summary>
        public int CurrentParticipants { get; set; }
        public int MaxParticipants { get; set; }

        /// <summary>
        /// Event status (Open, Confirming, Confirmed, etc.).
        /// </summary>
        public string Status { get; set; } = "Open";

        /// <summary>
        /// Match score based on user preferences (0-100).
        /// </summary>
        public int MatchScore { get; set; }

        /// <summary>
        /// Event creator information.
        /// </summary>
        public int CreatorId { get; set; }
        public required string CreatorName { get; set; }

        /// <summary>
        /// Distance from user's location in kilometers.
        /// </summary>
        public double? DistanceKm { get; set; }

        /// <summary>
        /// Time match rating (0-100).
        /// </summary>
        public int TimeMatchScore { get; set; }
    }
}

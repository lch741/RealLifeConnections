namespace backend.DTO.Feedback
{
    /// <summary>
    /// DTO for user feedback statistics and ratings.
    /// </summary>
    public class UserFeedbackStatsDto
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }

        /// <summary>
        /// Average rating from all feedback (0-5).
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of feedbacks received.
        /// </summary>
        public int TotalFeedbacks { get; set; }

        /// <summary>
        /// Percentage of users who would meet again.
        /// </summary>
        public double WouldMeetAgainPercentage { get; set; }

        /// <summary>
        /// Most common positive feedback theme.
        /// </summary>
        public string? PositiveTrend { get; set; }

        /// <summary>
        /// Most common suggestion from feedback.
        /// </summary>
        public string? SuggestionTrend { get; set; }

        /// <summary>
        /// Recent feedbacks (last 5).
        /// </summary>
        public List<MeetupFeedbackDto> RecentFeedbacks { get; set; } = new();
    }
}

namespace backend.DTO.Feedback
{
    /// <summary>
    /// DTO for meetup feedback response.
    /// </summary>
    public class MeetupFeedbackDto
    {
        public int Id { get; set; }
        public int MeetupEventId { get; set; }
        public required string MeetupTitle { get; set; }

        public int SubmittedByUserId { get; set; }
        public required string SubmittedByUserName { get; set; }

        /// <summary>
        /// Result: "Yes", "Okay", "No"
        /// </summary>
        public string Result { get; set; } = "Okay";

        public string? Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}

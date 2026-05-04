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

        public int FeedbackAboutUserId { get; set; }
        public required string FeedbackAboutUserName { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? PersonalityFeedback { get; set; }
        public bool? WouldMeetAgain { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

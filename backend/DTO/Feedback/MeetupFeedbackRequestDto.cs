using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Feedback
{
    /// <summary>
    /// DTO for submitting feedback for a completed meetup.
    /// </summary>
    public class MeetupFeedbackRequestDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        /// <summary>
        /// Optional personality trait feedback after the meetup.
        /// </summary>
        [MaxLength(200)]
        public string? PersonalityFeedback { get; set; }

        /// <summary>
        /// Would you meet this person again? (true/false)
        /// </summary>
        public bool? WouldMeetAgain { get; set; }
    }
}

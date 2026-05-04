using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Feedback
{
    /// <summary>
    /// DTO for submitting feedback for a completed meetup.
    /// </summary>
    public class MeetupFeedbackRequestDto
    {
        /// <summary>
        /// Result: "Yes", "Okay", "No"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public required string Result { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}

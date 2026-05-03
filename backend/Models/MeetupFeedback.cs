using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum MeetupFeedbackResult
    {
        Yes = 0,
        Okay = 1,
        No = 2
    }

    public class MeetupFeedback
    {
        public int Id { get; set; }

        public int MeetupEventId { get; set; }
        public MeetupEvent MeetupEvent { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; }

        public MeetupFeedbackResult Result { get; set; }

        [MaxLength(300)]
        public string? Comment { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
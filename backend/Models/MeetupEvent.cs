using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum MeetupStatus
    {
        Open = 0,
        Confirming = 1,
        Confirmed = 2,
        Completed = 3,
        Cancelled = 4
    }

    public class MeetupEvent
    {
        public int Id { get; set; }

        public int CreatorId { get; set; }
        public required AppUser Creator { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Location: Region + Suburb model
        [Required]
        [MaxLength(50)]
        public required string Region { get; set; }  // e.g., "New South Wales"

        [Required]
        [MaxLength(50)]
        public required string Suburb { get; set; }  // e.g., "Sydney", "Bondi"

        [MaxLength(100)]
        public string? LocationName { get; set; }  // e.g., "Bondi Beach", "Central Park"

        // Activity: One event = one activity
        public int ActivityId { get; set; }
        public required Activity Activity { get; set; }

        // Availability
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // Capacity
        public int MaxParticipants { get; set; } = 10;

        // Metadata
        public MeetupStatus Status { get; set; } = MeetupStatus.Open;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public List<UserMeetup> Participants { get; set; } = new();
        public List<Conversation> Conversations { get; set; } = new();
        public List<MeetupLocationSuggestion> LocationSuggestions { get; set; } = new();
        public List<MeetupFeedback> Feedbacks { get; set; } = new();
    }
}

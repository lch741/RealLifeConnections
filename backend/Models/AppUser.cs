using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2,
        NotToTell = 3
    }

    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        // Username (unique, 3-30 chars)
        [Required]
        [MaxLength(30)]
        public required string UserName { get; set; }

        // Password hash
        [Required]
        public required string PasswordHash { get; set; }

        // Location: Region + Suburb model (replaces City)
        [Required]
        [MaxLength(50)]
        public required string Region { get; set; }  // e.g., "New South Wales"

        [Required]
        [MaxLength(50)]
        public required string Suburb { get; set; }  // e.g., "Sydney", "Bondi"

        // Optional preferred distance for meetups (in km)
        public int? PreferredDistanceKm { get; set; }

        // Description
        [MaxLength(300)]
        public string? Bio { get; set; }

        // Profile image URL
        public string? ProfileImageUrl { get; set; }

        // Verification status
        public bool IsVerified { get; set; } = false;

        // Demographic fields
        public Gender Gender { get; set; } = Gender.NotToTell;

        // Age (nullable — can be omitted)
        public int? Age { get; set; }

        // Culture / nationality (e.g., "Canadian", "Japanese")
        [MaxLength(100)]
        public string? Culture { get; set; }

        // ========== PERSONALITY TRAITS (0-100 scale, nullable for MVP) ==========
        // Chill (0) ↔ Energetic (100)
        [Range(0, 100)]
        public int? ChillToEnergetic { get; set; }

        // Talkative (0) ↔ Quiet (100)
        [Range(0, 100)]
        public int? TalkativeToQuiet { get; set; }

        // Planner (0) ↔ Spontaneous (100)
        [Range(0, 100)]
        public int? PlannerToSpontaneous { get; set; }

        // Introvert (0) ↔ Extrovert (100)
        [Range(0, 100)]
        public int? IntrovertToExtrovert { get; set; }

        // ========== TIME AVAILABILITY PREFERENCES (optional) ==========
        // Preferred days: "Weekday", "Weekend", "Anytime"
        [MaxLength(50)]
        public string? PreferredDaysOfWeek { get; set; }

        // Preferred times: "Morning", "Afternoon", "Evening", "Night", "Anytime"
        [MaxLength(50)]
        public string? PreferredTimeOfDay { get; set; }

        // Navigation properties
        public List<UserInterest> Interests { get; set; } = new();
        public List<Verification> Verifications { get; set; } = new();
        public List<MeetupEvent> CreatedMeetups { get; set; } = new();
        public List<UserMeetup> JoinedMeetups { get; set; } = new();
        public List<MeetupLocationSuggestion> SuggestedLocations { get; set; } = new();
        public List<MeetupFeedback> MeetupFeedbacks { get; set; } = new();
        public List<Conversation> Conversations1 { get; set; } = new();
        public List<Conversation> Conversations2 { get; set; } = new();
        public List<Message> SentMessages { get; set; } = new();
    }
}
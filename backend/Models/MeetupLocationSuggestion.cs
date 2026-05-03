using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum MeetupLocationType
    {
        Cafe = 0,
        Park = 1,
        Restaurant = 2,
        Gym = 3,
        Bar = 4,
        Custom = 5
    }

    public class MeetupLocationSuggestion
    {
        public int Id { get; set; }

        public int MeetupEventId { get; set; }
        public MeetupEvent MeetupEvent { get; set; }

        public int SuggestedByUserId { get; set; }
        public AppUser SuggestedByUser { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public MeetupLocationType Type { get; set; } = MeetupLocationType.Custom;

        public bool IsChosen { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
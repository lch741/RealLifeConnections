using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public enum ActivityType
    {
        Cafe = 0,
        Park = 1,
        Restaurant = 2,
        Gym = 3,
        Bar = 4,
        Custom = 5,
    }

    public class Activity
    {
        public int Id { get; set; }

        public int MeetupEventId { get; set; }
        public required MeetupEvent MeetupEvent { get; set; }

        public ActivityType Type { get; set; } = ActivityType.Custom;

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }  // e.g., "Hiking", "Coffee", "Sports"

        [MaxLength(200)]
        public string? Description { get; set; }

        [Range(1, 3)]
        public int Order { get; set; }
    }
}


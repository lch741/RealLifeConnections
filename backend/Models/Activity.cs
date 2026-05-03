using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }  // e.g., "Hiking", "Coffee", "Sports"

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation property
        public List<MeetupEvent> MeetupEvents { get; set; } = new();
    }
}


using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for suggesting a location for a meetup event.
    /// </summary>
    public class SuggestLocationDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        /// <summary>
        /// Location type: "Cafe", "Park", "Restaurant", "Gym", "Bar", "Custom"
        /// </summary>
        [MaxLength(50)]
        public string Type { get; set; } = "Custom";
    }
}

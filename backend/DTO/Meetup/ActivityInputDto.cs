using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for meetup activity input.
    /// </summary>
    public class ActivityInputDto
    {
        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public string Type { get; set; } = "Custom";

        [Range(1, 3)]
        public int Order { get; set; }
    }
}

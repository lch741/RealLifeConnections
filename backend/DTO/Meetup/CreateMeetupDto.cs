using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for creating a new meetup event.
    /// </summary>
    public class CreateMeetupDto
    {
        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Region { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Suburb { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Range(1, 1000)]
        public int MaxParticipants { get; set; } = 10;
    }
}

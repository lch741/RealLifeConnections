using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for updating a meetup event.
    /// </summary>
    public class UpdateMeetupDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; }

        [MaxLength(50)]
        public string? Suburb { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(3)]
        public List<ActivityInputDto>? Activities { get; set; }

        public DateTime? EventDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Range(1, 1000)]
        public int? MaxParticipants { get; set; }

        [Range(1, 1000)]
        public int? MaxDistanceKm { get; set; }
    }
}

namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for meetup event response.
    /// </summary>
    public class MeetupEventDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string Region { get; set; }
        public required string Suburb { get; set; }
        public string? LocationName { get; set; }

        public int ActivityId { get; set; }
        public required string ActivityName { get; set; }
        public string? ActivityCategory { get; set; }

        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }

        public string Status { get; set; } = "Open";
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int CreatorId { get; set; }
        public required string CreatorName { get; set; }

        public List<UserMeetupDto> Participants { get; set; } = new();
        public List<MeetupLocationSuggestionDto> LocationSuggestions { get; set; } = new();
    }
}

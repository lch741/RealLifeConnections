namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for meetup location suggestion response.
    /// </summary>
    public class MeetupLocationSuggestionDto
    {
        public int Id { get; set; }
        public int MeetupEventId { get; set; }
        public int SuggestedByUserId { get; set; }
        public required string SuggestedByUserName { get; set; }

        public required string Name { get; set; }
        public string? Address { get; set; }
        public string Type { get; set; } = "Custom";
        public bool IsChosen { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

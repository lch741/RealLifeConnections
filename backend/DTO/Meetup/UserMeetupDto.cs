namespace backend.DTO.Meetup
{
    /// <summary>
    /// DTO for user participation in a meetup event.
    /// </summary>
    public class UserMeetupDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Status: "Pending", "Approved", "Rejected", "Left"
        /// </summary>
        public string Status { get; set; } = "Pending";

        public DateTime JoinedAt { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }
}

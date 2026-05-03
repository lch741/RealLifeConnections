using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int? MeetupEventId { get; set; }
        public MeetupEvent? MeetupEvent { get; set; }

        public int User1Id { get; set; }
        public AppUser User1 { get; set; }

        public int User2Id { get; set; }
        public AppUser User2 { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndsAt { get; set; }
        public bool IsClosed { get; set; } = false;
        public DateTime? ClosedAt { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public List<Message> Messages { get; set; } = new();
    }
}
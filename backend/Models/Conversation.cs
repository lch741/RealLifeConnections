using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public int User1Id { get; set; }
        public AppUser User1 { get; set; }

        public int User2Id { get; set; }
        public AppUser User2 { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public List<Message> Messages { get; set; } = new();
    }
}
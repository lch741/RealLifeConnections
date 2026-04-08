using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public required Conversation Conversation { get; set; }

        // 发送者
        public int SenderId { get; set; }
        public required AppUser Sender { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 是否已读
        public bool IsRead { get; set; } = false;
    }
}
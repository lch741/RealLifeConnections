using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public int OtherUserId { get; set; }
        public string? OtherUserName { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool IsClosed { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? EndsAt { get; set; }
    }
}
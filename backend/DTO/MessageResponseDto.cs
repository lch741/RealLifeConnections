using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
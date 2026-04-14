using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class SendMessageDto
    {
        public int ReceiverId { get; set; }
        public required string Content { get; set; }
    }
}
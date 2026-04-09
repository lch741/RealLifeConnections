using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using backend.DTOs;

namespace backend.Interfaces
{
    public interface IChatService
    {
        Task SendMessageAsync(int senderId, SendMessageDto dto);
        Task<List<MessageResponseDto>> GetMessagesAsync(int userId, int otherUserId);
        Task<List<ConversationDto>> GetConversationsAsync(int userId);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace backend.Interfaces
{
    public interface IChatRepository
    {
        Task<Conversation?> GetConversationAsync(int user1Id, int user2Id);
        Task<Conversation> CreateConversationAsync(int user1Id, int user2Id);
        Task<Message> AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesAsync(int conversationId);
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
    }
}
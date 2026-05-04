using backend.Models;
using backend.Data;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDBContext _context;

        public ChatRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetConversationAsync(int user1Id, int user2Id)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));
        }

        public async Task<Conversation> CreateConversationAsync(int user1Id, int user2Id)
        {
            var convo = new Conversation
            {
                User1Id = user1Id,
                User2Id = user2Id
            };

            _context.Conversations.Add(convo);
            await _context.SaveChangesAsync();
            return convo;
        }

        public async Task<Conversation> CreateConversationAsync(int user1Id, int user2Id, int? meetupEventId, DateTime? endsAt)
        {
            var convo = new Conversation
            {
                User1Id = user1Id,
                User2Id = user2Id,
                MeetupEventId = meetupEventId,
                StartedAt = DateTime.UtcNow,
                EndsAt = endsAt
            };

            _context.Conversations.Add(convo);
            await _context.SaveChangesAsync();
            return convo;
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesAsync(int conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }
    }
}
using backend.DTOs;
using backend.Data;
using backend.Interfaces;
using api.Models;
using api.DTOs;

namespace backend.Service
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;

        public ChatService(IChatRepository repo)
        {
            _repo = repo;
        }

        public async Task SendMessageAsync(int senderId, SendMessageDto dto)
        {
            var convo = await _repo.GetConversationAsync(senderId, dto.ReceiverId);

            if (convo == null)
            {
                convo = await _repo.CreateConversationAsync(senderId, dto.ReceiverId);
            }

            var message = new Message
            {
                ConversationId = convo.Id,
                SenderId = senderId,
                Content = dto.Content
            };

            await _repo.AddMessageAsync(message);

            convo.LastMessageAt = DateTime.UtcNow;
        }

        public async Task<List<MessageResponseDto>> GetMessagesAsync(int userId, int otherUserId)
        {
            var convo = await _repo.GetConversationAsync(userId, otherUserId);
            if (convo == null) return new List<MessageResponseDto>();

            var messages = await _repo.GetMessagesAsync(convo.Id);

            return messages.Select(m => new MessageResponseDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
        {
            var convos = await _repo.GetUserConversationsAsync(userId);

            return convos.Select(c => new ConversationDto
            {
                ConversationId = c.Id,
                OtherUserId = c.User1Id == userId ? c.User2Id : c.User1Id,
                LastMessageAt = c.LastMessageAt
            }).ToList();
        }
    }
}
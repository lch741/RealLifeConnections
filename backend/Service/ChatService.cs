using backend.DTOs;
using backend.Data;
using backend.Interfaces;
using api.DTOs;
using backend.Models;
using backend.Mapper;

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

            return messages.Select(ChatMapper.ToMessageResponse).ToList();
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
        {
            var convos = await _repo.GetUserConversationsAsync(userId);

            return convos.Select(conversation => ChatMapper.ToConversation(conversation, userId)).ToList();
        }
    }
}
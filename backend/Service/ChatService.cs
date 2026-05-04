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

            // If the conversation is linked to a meetup and has an expiry, enforce time-limited chat
            if (convo.IsClosed)
            {
                throw new InvalidOperationException("This conversation is closed.");
            }

            if (convo.MeetupEventId.HasValue && convo.EndsAt.HasValue && convo.EndsAt.Value < DateTime.UtcNow)
            {
                // Mark closed for clarity
                convo.IsClosed = true;
                // update last-closed fields if repository exposes update (keep in-memory flag update minimal)
                throw new InvalidOperationException("Chat window for this meetup has expired.");
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
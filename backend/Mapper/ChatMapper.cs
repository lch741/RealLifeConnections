using backend.DTOs;
using api.DTOs;
using api.Models;

namespace backend.Mapper
{
    public static class ChatMapper
    {
        public static MessageResponseDto ToMessageResponse(Message message)
        {
            return new MessageResponseDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };
        }

        public static ConversationDto ToConversation(Conversation conversation, int currentUserId)
        {
            return new ConversationDto
            {
                ConversationId = conversation.Id,
                OtherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id,
                OtherUserName = string.Empty,
                LastMessageAt = conversation.LastMessageAt
            };
        }
    }
}
using backend.DTOs;
using api.DTOs;
using backend.Models;

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
            var meetupEndsAt = GetMeetupEndsAt(conversation);
            return new ConversationDto
            {
                ConversationId = conversation.Id,
                OtherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id,
                OtherUserName = string.Empty,
                LastMessageAt = conversation.LastMessageAt,
                IsClosed = conversation.IsClosed,
                EndsAt = meetupEndsAt ?? conversation.EndsAt,
                IsExpired = meetupEndsAt.HasValue && meetupEndsAt.Value < DateTime.UtcNow
            };
        }

        private static DateTime? GetMeetupEndsAt(Conversation conversation)
        {
            var meetup = conversation.MeetupEvent;
            if (meetup == null)
            {
                return conversation.EndsAt;
            }

            if (meetup.Status == MeetupStatus.Completed && meetup.CompletedAt.HasValue)
            {
                return meetup.CompletedAt.Value;
            }

            if (!meetup.EndTime.HasValue)
            {
                return null;
            }

            var baseDate = meetup.EventDate.Date;
            var endsAt = baseDate.Add(meetup.EndTime.Value);
            if (meetup.EndTime.Value < meetup.StartTime)
            {
                endsAt = endsAt.AddDays(1);
            }

            return DateTime.SpecifyKind(endsAt, DateTimeKind.Utc);
        }
    }
}
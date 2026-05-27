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
        private readonly IMeetupRepository _meetupRepository;

        public ChatService(IChatRepository repo, IMeetupRepository meetupRepository)
        {
            _repo = repo;
            _meetupRepository = meetupRepository;
        }

        public async Task SendMessageAsync(int senderId, SendMessageDto dto)
        {
            if (senderId == dto.ReceiverId)
            {
                throw new InvalidOperationException("Cannot message yourself.");
            }

            Conversation? convo;
            if (dto.MeetupEventId.HasValue)
            {
                var meetup = await _meetupRepository.GetByIdAsync(dto.MeetupEventId.Value);
                EnsureMeetupChatAccess(senderId, dto.ReceiverId, meetup);

                if (HasMeetupEnded(meetup))
                {
                    throw new InvalidOperationException("Chat window for this meetup has expired.");
                }

                convo = await _repo.GetMeetupConversationAsync(senderId, dto.ReceiverId, meetup.Id);
                if (convo == null)
                {
                    convo = await _repo.CreateConversationAsync(
                        senderId,
                        dto.ReceiverId,
                        meetup.Id,
                        GetMeetupEndsAt(meetup));
                }
            }
            else
            {
                convo = await _repo.GetConversationAsync(senderId, dto.ReceiverId);
            }

            if (convo == null)
            {
                convo = await _repo.CreateConversationAsync(senderId, dto.ReceiverId);
            }

            // If the conversation is linked to a meetup and has an expiry, enforce time-limited chat
            if (convo.IsClosed)
            {
                throw new InvalidOperationException("This conversation is closed.");
            }

            var meetupEndsAt = GetMeetupEndsAt(convo);
            if (convo.MeetupEventId.HasValue && meetupEndsAt.HasValue && meetupEndsAt.Value < DateTime.UtcNow)
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
            await _repo.UpdateConversationAsync(convo);
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

        private static DateTime? GetMeetupEndsAt(MeetupEvent meetup)
        {
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

        private static bool HasMeetupEnded(MeetupEvent meetup)
        {
            if (meetup.Status == MeetupStatus.Cancelled)
            {
                return true;
            }

            var endsAt = GetMeetupEndsAt(meetup);
            return endsAt.HasValue && endsAt.Value < DateTime.UtcNow;
        }

        private static void EnsureMeetupChatAccess(int senderId, int receiverId, MeetupEvent meetup)
        {
            var creatorId = meetup.CreatorId;
            var approvedParticipants = meetup.Participants
                .Where(participant => participant.Status == UserMeetup.ParticipantStatus.Approved)
                .Select(participant => participant.UserId)
                .ToHashSet();

            if (senderId == creatorId)
            {
                if (!approvedParticipants.Contains(receiverId))
                {
                    throw new UnauthorizedAccessException("Only approved participants can chat with the host.");
                }

                return;
            }

            if (receiverId == creatorId && approvedParticipants.Contains(senderId))
            {
                return;
            }

            throw new UnauthorizedAccessException("Meetup chat is only available between the host and approved participants.");
        }

        public async Task<List<MessageResponseDto>> GetMessagesAsync(int userId, int otherUserId)
        {
            var convo = await _repo.GetConversationAsync(userId, otherUserId);
            if (convo == null) return new List<MessageResponseDto>();

            // Allow viewing messages even if the chat is expired or closed; sending is blocked elsewhere.
            var messages = await _repo.GetMessagesAsync(convo.Id);

            return messages.Select(ChatMapper.ToMessageResponse).ToList();
        }

        public async Task<List<MessageResponseDto>> GetMeetupMessagesAsync(int userId, int otherUserId, int meetupId)
        {
            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureMeetupChatAccess(userId, otherUserId, meetup);
            var convo = await _repo.GetMeetupConversationAsync(userId, otherUserId, meetupId);
            if (convo == null) return new List<MessageResponseDto>();

            var messages = await _repo.GetMessagesAsync(convo.Id);
            return messages.Select(ChatMapper.ToMessageResponse).ToList();
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
        {
            var convos = await _repo.GetUserConversationsAsync(userId);

            // Return all conversations (including expired/closed) so user can view history; client should use flags to disable sending.
            return convos.Select(conversation => ChatMapper.ToConversation(conversation, userId)).ToList();
        }
    }
}
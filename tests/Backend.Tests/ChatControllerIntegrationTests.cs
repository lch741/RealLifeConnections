using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api.DTOs;
using backend.DTO.Meetup;
using backend.DTOs;
using Xunit;

namespace Backend.Tests
{
    public class ChatControllerIntegrationTests
    {
        [Fact]
        public async Task SendMessageAndGetConversation_ReturnsMessages()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var senderClient = factory.CreateClient();
            using var receiverClient = factory.CreateClient();

            var senderAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(senderClient, "chat-sender@example.com", "chatsender");
            var receiverAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(receiverClient, "chat-receiver@example.com", "chatreceiver");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, senderAuth.Email);
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, receiverAuth.Email);

            senderClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", senderAuth.Token);

            var receiverId = await IntegrationTestHelpers.GetUserIdAsync(factory, receiverAuth.Email);

            var sendResponse = await senderClient.PostAsJsonAsync("/api/chat/send", new SendMessageDto
            {
                ReceiverId = receiverId,
                Content = "Hello from integration tests"
            });

            Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);

            var messagesResponse = await senderClient.GetAsync($"/api/chat/{receiverId}");
            Assert.Equal(HttpStatusCode.OK, messagesResponse.StatusCode);

            var messages = await messagesResponse.Content.ReadFromJsonAsync<MessageResponseDto[]>();
            Assert.NotNull(messages);
            Assert.Contains(messages!, message => message.Content == "Hello from integration tests");

            var conversationsResponse = await senderClient.GetAsync("/api/chat/conversations");
            Assert.Equal(HttpStatusCode.OK, conversationsResponse.StatusCode);

            var conversations = await conversationsResponse.Content.ReadFromJsonAsync<ConversationDto[]>();
            Assert.NotNull(conversations);
            Assert.Contains(conversations!, convo => convo.OtherUserId == receiverId);
        }

        [Fact]
        public async Task SendMeetupMessageAndGetMeetupMessages_ReturnsMessages()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var creatorClient = factory.CreateClient();
            using var participantClient = factory.CreateClient();

            var creatorAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(creatorClient, "chat-meetup-host@example.com", "chatmeetuphost");
            var participantAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(participantClient, "chat-meetup-user@example.com", "chatmeetupuser");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, creatorAuth.Email);
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, participantAuth.Email);

            creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAuth.Token);
            participantClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participantAuth.Token);

            var meetup = await CreateMeetupAsync(creatorClient);

            var applyResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/apply", null);
            Assert.Equal(HttpStatusCode.OK, applyResponse.StatusCode);

            var participantId = await IntegrationTestHelpers.GetUserIdAsync(factory, participantAuth.Email);
            var creatorId = await IntegrationTestHelpers.GetUserIdAsync(factory, creatorAuth.Email);
            var approveResponse = await creatorClient.PostAsync($"/api/meetups/{meetup.Id}/approve/{participantId}", null);
            Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

            var sendResponse = await creatorClient.PostAsJsonAsync("/api/chat/send", new SendMessageDto
            {
                ReceiverId = participantId,
                MeetupEventId = meetup.Id,
                Content = "Meetup chat message"
            });

            Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);

            var messagesResponse = await creatorClient.GetAsync($"/api/chat/meetups/{meetup.Id}/{participantId}");
            Assert.Equal(HttpStatusCode.OK, messagesResponse.StatusCode);

            var messages = await messagesResponse.Content.ReadFromJsonAsync<MessageResponseDto[]>();
            Assert.NotNull(messages);
            Assert.Contains(messages!, message => message.Content == "Meetup chat message");

            var participantMessagesResponse = await participantClient.GetAsync($"/api/chat/meetups/{meetup.Id}/{creatorId}");
            Assert.Equal(HttpStatusCode.OK, participantMessagesResponse.StatusCode);

            var participantMessages = await participantMessagesResponse.Content.ReadFromJsonAsync<MessageResponseDto[]>();
            Assert.NotNull(participantMessages);
            Assert.Contains(participantMessages!, message => message.Content == "Meetup chat message");
        }

        private static async Task<MeetupEventDto> CreateMeetupAsync(HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/meetups", new CreateMeetupDto
            {
                Title = "Integration test meetup",
                Description = "Integration test description",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                LocationName = "Test Location",
                Activities = new System.Collections.Generic.List<ActivityInputDto>
                {
                    new ActivityInputDto { Name = "Coffee", Order = 1 }
                },
                EventDate = DateTime.UtcNow.Date.AddDays(2),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                MaxParticipants = 5
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var meetup = await response.Content.ReadFromJsonAsync<MeetupEventDto>();
            Assert.NotNull(meetup);
            return meetup!;
        }
    }
}

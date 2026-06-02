using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using backend.DTO.Meetup;
using Xunit;

namespace Backend.Tests
{
    public class MeetupControllerIntegrationTests
    {
        [Fact]
        public async Task CreateMeetupAndGetCreatedByMe_ReturnsCreatedMeetup()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var client = factory.CreateClient();

            var auth = await IntegrationTestHelpers.RegisterAndLoginAsync(client, "meetup-user@example.com", "meetupuser");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, auth.Email);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var created = await CreateMeetupAsync(client);

            var createdResponse = await client.GetAsync("/api/meetups/created");
            Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);

            var createdMeetups = await createdResponse.Content.ReadFromJsonAsync<MeetupEventDto[]>();
            Assert.NotNull(createdMeetups);
            Assert.Contains(createdMeetups!, item => item.Id == created.Id);
        }

        [Fact]
        public async Task UpdateMeetup_ChangesTitleAndCapacity()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var client = factory.CreateClient();

            var auth = await IntegrationTestHelpers.RegisterAndLoginAsync(client, "meetup-update@example.com", "meetupupdate");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, auth.Email);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var created = await CreateMeetupAsync(client);

            var updateResponse = await client.PutAsJsonAsync($"/api/meetups/{created.Id}", new UpdateMeetupDto
            {
                Title = "Updated meetup title",
                MaxParticipants = 20
            });

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updated = await updateResponse.Content.ReadFromJsonAsync<MeetupEventDto>();
            Assert.NotNull(updated);
            Assert.Equal("Updated meetup title", updated!.Title);
            Assert.Equal(20, updated.MaxParticipants);
        }

        [Fact]
        public async Task UpdateMeetupStatus_ConfirmsMeetup()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var client = factory.CreateClient();

            var auth = await IntegrationTestHelpers.RegisterAndLoginAsync(client, "meetup-status@example.com", "meetupstatus");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, auth.Email);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var created = await CreateMeetupAsync(client);

            var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/meetups/{created.Id}/status?status=Confirmed");
            var statusResponse = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

            var updated = await statusResponse.Content.ReadFromJsonAsync<MeetupEventDto>();
            Assert.NotNull(updated);
            Assert.Equal("Confirmed", updated!.Status);
            Assert.NotNull(updated.ConfirmedAt);
        }

        [Fact]
        public async Task ApplyMeetupAndGetJoinedByMe_ReturnsJoinedMeetup()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var creatorClient = factory.CreateClient();
            using var participantClient = factory.CreateClient();

            var creatorAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(creatorClient, "meetup-host@example.com", "meetuphost");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, creatorAuth.Email);
            creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAuth.Token);

            var meetup = await CreateMeetupAsync(creatorClient);

            var participantAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(participantClient, "meetup-joiner@example.com", "meetupjoiner");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, participantAuth.Email);
            participantClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participantAuth.Token);

            var applyResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/apply", null);
            Assert.Equal(HttpStatusCode.OK, applyResponse.StatusCode);

            var joinedResponse = await participantClient.GetAsync("/api/meetups/joined");
            Assert.Equal(HttpStatusCode.OK, joinedResponse.StatusCode);

            var joinedMeetups = await joinedResponse.Content.ReadFromJsonAsync<MeetupEventDto[]>();
            Assert.NotNull(joinedMeetups);
            Assert.Contains(joinedMeetups!, item => item.Id == meetup.Id);
        }

        [Fact]
        public async Task QuitMeetup_UpdatesParticipantStatus()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var creatorClient = factory.CreateClient();
            using var participantClient = factory.CreateClient();

            var creatorAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(creatorClient, "meetup-quit-host@example.com", "meetupquithost");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, creatorAuth.Email);
            creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAuth.Token);

            var meetup = await CreateMeetupAsync(creatorClient);

            var participantAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(participantClient, "meetup-quit-user@example.com", "meetupquituser");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, participantAuth.Email);
            participantClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participantAuth.Token);

            var applyResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/apply", null);
            Assert.Equal(HttpStatusCode.OK, applyResponse.StatusCode);

            var quitResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/quit", null);
            Assert.Equal(HttpStatusCode.OK, quitResponse.StatusCode);

            var refreshedResponse = await creatorClient.GetAsync($"/api/meetups/{meetup.Id}");
            Assert.Equal(HttpStatusCode.OK, refreshedResponse.StatusCode);

            var refreshed = await refreshedResponse.Content.ReadFromJsonAsync<MeetupEventDto>();
            Assert.NotNull(refreshed);
            Assert.Contains(refreshed!.Participants, participant => participant.Status == "Left");
        }

        [Fact]
        public async Task ApproveParticipant_UpdatesParticipantStatus()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var creatorClient = factory.CreateClient();
            using var participantClient = factory.CreateClient();

            var creatorAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(creatorClient, "meetup-approve-host@example.com", "meetupapprovehost");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, creatorAuth.Email);
            creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAuth.Token);

            var meetup = await CreateMeetupAsync(creatorClient);

            var participantAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(participantClient, "meetup-approve-user@example.com", "meetupapproveuser");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, participantAuth.Email);
            participantClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participantAuth.Token);

            var applyResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/apply", null);
            Assert.Equal(HttpStatusCode.OK, applyResponse.StatusCode);

            var participantId = await IntegrationTestHelpers.GetUserIdAsync(factory, participantAuth.Email);
            var approveResponse = await creatorClient.PostAsync($"/api/meetups/{meetup.Id}/approve/{participantId}", null);
            Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

            var approved = await approveResponse.Content.ReadFromJsonAsync<UserMeetupDto>();
            Assert.NotNull(approved);
            Assert.Equal("Approved", approved!.Status);
            Assert.True(approved.IsConfirmed);
        }

        [Fact]
        public async Task RejectParticipant_UpdatesParticipantStatus()
        {
            using var factory = IntegrationTestHelpers.CreateFactory();
            using var creatorClient = factory.CreateClient();
            using var participantClient = factory.CreateClient();

            var creatorAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(creatorClient, "meetup-reject-host@example.com", "meetuprejecthost");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, creatorAuth.Email);
            creatorClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", creatorAuth.Token);

            var meetup = await CreateMeetupAsync(creatorClient);

            var participantAuth = await IntegrationTestHelpers.RegisterAndLoginAsync(participantClient, "meetup-reject-user@example.com", "meetuprejectuser");
            await IntegrationTestHelpers.MarkUserVerifiedAsync(factory, participantAuth.Email);
            participantClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participantAuth.Token);

            var applyResponse = await participantClient.PostAsync($"/api/meetups/{meetup.Id}/apply", null);
            Assert.Equal(HttpStatusCode.OK, applyResponse.StatusCode);

            var participantId = await IntegrationTestHelpers.GetUserIdAsync(factory, participantAuth.Email);
            var rejectResponse = await creatorClient.PostAsync($"/api/meetups/{meetup.Id}/reject/{participantId}", null);
            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var refreshedResponse = await creatorClient.GetAsync($"/api/meetups/{meetup.Id}");
            Assert.Equal(HttpStatusCode.OK, refreshedResponse.StatusCode);

            var refreshed = await refreshedResponse.Content.ReadFromJsonAsync<MeetupEventDto>();
            Assert.NotNull(refreshed);
            Assert.Contains(refreshed!.Participants, participant => participant.Status == "Rejected");
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
                Activities = new List<ActivityInputDto>
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
            Assert.Equal("Integration test meetup", meetup!.Title);
            return meetup;
        }
    }
}

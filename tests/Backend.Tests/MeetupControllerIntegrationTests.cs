using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.DTO.Meetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            await MarkUserVerifiedAsync(factory, auth.Email);
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
            await MarkUserVerifiedAsync(factory, auth.Email);
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
            await MarkUserVerifiedAsync(factory, auth.Email);
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

        private static async Task MarkUserVerifiedAsync(IntegrationWebApplicationFactory factory, string email)
        {
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.SingleAsync(user => user.Email == email.ToLowerInvariant());
            storedUser.IsVerified = true;
            context.Users.Update(storedUser);
            await context.SaveChangesAsync();
        }

    }
}

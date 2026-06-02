using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests
{
    internal static class IntegrationTestHelpers
    {
        internal static async Task<AuthenticatedUser> RegisterAndLoginAsync(HttpClient client, string email, string userName)
        {
            var registerDto = new RegisterUserDto
            {
                Email = email,
                UserName = userName,
                Password = "Password123!!",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                InterestSelections = new List<RegisterInterestSelectionDto>
                {
                    new RegisterInterestSelectionDto
                    {
                        CategoryId = 1,
                        Interests = new List<string> { "Coffee" }
                    }
                }
            };

            var registerResponse = await client.PostAsJsonAsync("/api/user/register", registerDto);
            Assert.Equal(System.Net.HttpStatusCode.OK, registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync("/api/user/login", new LoginUserDto
            {
                Email = email,
                Password = registerDto.Password
            });

            Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);

            var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);

            return new AuthenticatedUser(email, userName, authResponse!.Token);
        }

        internal static IntegrationWebApplicationFactory CreateFactory()
        {
            var factory = new IntegrationWebApplicationFactory();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            context.Database.EnsureCreated();
            return factory;
        }

        internal static async Task MarkUserVerifiedAsync(IntegrationWebApplicationFactory factory, string email)
        {
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.SingleAsync(user => user.Email == email.ToLowerInvariant());
            storedUser.IsVerified = true;
            context.Users.Update(storedUser);
            await context.SaveChangesAsync();
        }

        internal static async Task<int> GetUserIdAsync(IntegrationWebApplicationFactory factory, string email)
        {
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.SingleAsync(user => user.Email == email.ToLowerInvariant());
            return storedUser.Id;
        }

        internal sealed record AuthenticatedUser(string Email, string UserName, string Token);
    }
}

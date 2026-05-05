using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Interfaces;
using backend.Models;
using backend.Repository;
using backend.Service;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using BackendProgram = global::Program;

namespace Backend.Tests
{
    public class UserControllerIntegrationTests
    {
        [Fact]
        public async Task RegisterAndLogin_PersistsUserAndReturnsToken()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var registerDto = new RegisterUserDto
            {
                Email = "integration-user@example.com",
                UserName = "integrationuser",
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

            var registerResponseMessage = await client.PostAsJsonAsync("/api/user/register", registerDto);
            Assert.Equal(HttpStatusCode.OK, registerResponseMessage.StatusCode);

            var registerResponse = await registerResponseMessage.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(registerResponse);
            Assert.False(string.IsNullOrWhiteSpace(registerResponse!.Token));
            Assert.Equal(registerDto.Email.ToLowerInvariant(), registerResponse.User.Email);
            Assert.Equal(registerDto.UserName, registerResponse.User.UserName);

            var loginResponseMessage = await client.PostAsJsonAsync("/api/user/login", new LoginUserDto
            {
                Email = registerDto.Email,
                Password = registerDto.Password
            });

            Assert.Equal(HttpStatusCode.OK, loginResponseMessage.StatusCode);

            var loginResponse = await loginResponseMessage.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrWhiteSpace(loginResponse!.Token));
            Assert.Equal(registerDto.Email.ToLowerInvariant(), loginResponse.User.Email);

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.SingleAsync(user => user.Email == registerDto.Email.ToLowerInvariant());
            Assert.Equal(registerDto.UserName, storedUser.UserName);
            Assert.True(VerifyPassword(storedUser.PasswordHash, registerDto.Password));
        }

        [Fact]
        public async Task GetAndUpdateProfile_ReturnsUpdatedProfileData()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var auth = await RegisterAndLoginAsync(client, "profile-user@example.com", "profileuser");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var profileResponseMessage = await client.GetAsync("/api/user/profile");
            Assert.Equal(HttpStatusCode.OK, profileResponseMessage.StatusCode);

            var initialProfile = await profileResponseMessage.Content.ReadFromJsonAsync<UserProfileDto>();
            Assert.NotNull(initialProfile);
            Assert.Equal(auth.Email.ToLowerInvariant(), initialProfile!.Email);
            Assert.Equal(auth.UserName, initialProfile.UserName);

            var updateResponseMessage = await client.PutAsJsonAsync("/api/user/profile", new UpdateProfileDto
            {
                UserName = "profileuserupdated",
                Bio = "Updated bio",
                Region = "UpdatedRegion",
                Suburb = "UpdatedSuburb",
                Gender = "Female",
                Age = 31,
                Culture = "UpdatedCulture",
                InterestSelections = new List<RegisterInterestSelectionDto>
                {
                    new RegisterInterestSelectionDto
                    {
                        CategoryId = 2,
                        Interests = new List<string> { "Painting" }
                    }
                }
            });

            Assert.Equal(HttpStatusCode.OK, updateResponseMessage.StatusCode);

            var updatedProfile = await updateResponseMessage.Content.ReadFromJsonAsync<UserProfileDto>();
            Assert.NotNull(updatedProfile);
            Assert.Equal("profileuserupdated", updatedProfile!.UserName);
            Assert.Equal("Updated bio", updatedProfile.Bio);
            Assert.Equal("UpdatedRegion", updatedProfile.Region);
            Assert.Equal("UpdatedSuburb", updatedProfile.Suburb);
            Assert.Equal("Female", updatedProfile.Gender);
            Assert.Equal(31, updatedProfile.Age);
            Assert.Equal("UpdatedCulture", updatedProfile.Culture);

            var refreshedProfileResponseMessage = await client.GetAsync("/api/user/profile");
            Assert.Equal(HttpStatusCode.OK, refreshedProfileResponseMessage.StatusCode);

            var refreshedProfile = await refreshedProfileResponseMessage.Content.ReadFromJsonAsync<UserProfileDto>();
            Assert.NotNull(refreshedProfile);
            Assert.Equal("profileuserupdated", refreshedProfile!.UserName);
            Assert.Equal("UpdatedRegion", refreshedProfile.Region);
            Assert.Equal("UpdatedSuburb", refreshedProfile.Suburb);

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.Include(current => current.Interests).SingleAsync(current => current.Email == auth.Email.ToLowerInvariant());
            Assert.Equal("profileuserupdated", storedUser.UserName);
            Assert.Equal("UpdatedRegion", storedUser.Region);
            Assert.Equal("UpdatedSuburb", storedUser.Suburb);
            Assert.Equal(Gender.Female, storedUser.Gender);
            Assert.Equal(31, storedUser.Age);
            Assert.Equal("UpdatedCulture", storedUser.Culture);
            Assert.Single(storedUser.Interests);
            Assert.Equal("Painting", storedUser.Interests[0].SubCategory);
        }

        [Fact]
        public async Task SaveAvatarAndVerifyFace_StoresAvatarAndApprovesVerification()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var auth = await RegisterAndLoginAsync(client, "avatar-user@example.com", "avataruser");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

            var avatarFile = CreateImageFile("avatar.jpg", "image/jpeg", "avatar-bytes");
            var saveAvatarResponse = await PostMultipartAsync(client, "/api/user/avatar/upload", avatarFile, "avatarFile");

            Assert.Equal(HttpStatusCode.OK, saveAvatarResponse.StatusCode);

            var avatarProfile = await saveAvatarResponse.Content.ReadFromJsonAsync<UserProfileDto>();
            Assert.NotNull(avatarProfile);
            Assert.StartsWith("data:image/jpeg;base64,", avatarProfile!.AvatarUrl);

            var liveCaptureFile = CreateImageFile("livecapture.jpg", "image/jpeg", "live-capture-bytes");
            factory.FaceServiceMock
                .Setup(service => service.VerifyFacesAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .ReturnsAsync(new AzureFaceVerificationResultDto
                {
                    IsIdentical = true,
                    Confidence = 0.94
                });

            var verifyFaceResponse = await PostMultipartAsync(client, "/api/user/verify-face/upload", liveCaptureFile, "liveCaptureFile");

            Assert.Equal(HttpStatusCode.OK, verifyFaceResponse.StatusCode);

            var verificationResponse = await verifyFaceResponse.Content.ReadFromJsonAsync<FaceVerificationResponseDto>();
            Assert.NotNull(verificationResponse);
            Assert.True(verificationResponse!.IsVerified);
            Assert.Equal("approved", verificationResponse.Status);
            Assert.Equal(0.94, verificationResponse.Confidence, 2);

            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var storedUser = await context.Users.Include(current => current.Verifications).SingleAsync(current => current.Email == auth.Email.ToLowerInvariant());
            Assert.True(storedUser.IsVerified);
            Assert.StartsWith("data:image/jpeg;base64,", storedUser.ProfileImageUrl!, StringComparison.Ordinal);
            Assert.Equal(2, storedUser.Verifications.Count);
        }

        private static async Task<AuthenticatedUser> RegisterAndLoginAsync(HttpClient client, string email, string userName)
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
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync("/api/user/login", new LoginUserDto
            {
                Email = email,
                Password = registerDto.Password
            });

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);

            return new AuthenticatedUser(email, userName, authResponse!.Token);
        }

        private static IntegrationWebApplicationFactory CreateFactory()
        {
            var factory = new IntegrationWebApplicationFactory();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            context.Database.EnsureCreated();
            return factory;
        }

        private static IFormFile CreateImageFile(string fileName, string contentType, string contents)
        {
            var fileBytes = Encoding.UTF8.GetBytes(contents);
            var stream = new MemoryStream(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        private static bool VerifyPassword(string storedPasswordHash, string password)
        {
            var parts = storedPasswordHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var actualHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                expectedHash.Length);

            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private sealed record AuthenticatedUser(string Email, string UserName, string Token);

        private static async Task<HttpResponseMessage> PostMultipartAsync(HttpClient client, string requestUri, IFormFile file, string formFieldName)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, formFieldName, file.FileName);
            return await client.PostAsync(requestUri, content);
        }
    }
}
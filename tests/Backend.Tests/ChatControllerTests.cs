using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.DTOs;
using backend.Controllers;
using backend.DTOs;
using backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class ChatControllerTests
    {
        private readonly Mock<IChatService> _mockChatService;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _mockChatService = new Mock<IChatService>();
            _controller = new ChatController(_mockChatService.Object);
        }

        private void SetUser(string userId = "1")
        {
            var userClaims = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };
        }

        private static string? GetMessagePayloadValue(object? payload, string propertyName)
        {
            var property = payload?.GetType().GetProperty(propertyName);
            return property?.GetValue(payload)?.ToString();
        }

        [Fact]
        public async Task SendMessage_ReturnsOkResult_WhenSendSucceeds()
        {
            SetUser();
            var dto = new SendMessageDto
            {
                ReceiverId = 2,
                Content = "Hello"
            };

            _mockChatService
                .Setup(s => s.SendMessageAsync(1, dto))
                .Returns(Task.CompletedTask);

            var result = await _controller.SendMessage(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Message sent successfully", GetMessagePayloadValue(ok.Value, "message"));
        }

        [Fact]
        public async Task SendMessage_ReturnsBadRequest_WhenContentMissing()
        {
            SetUser();
            var dto = new SendMessageDto
            {
                ReceiverId = 2,
                Content = " "
            };

            var result = await _controller.SendMessage(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Message content is required", badRequest.Value);
        }

        [Fact]
        public async Task SendMessage_ReturnsBadRequest_WhenUserIdInvalid()
        {
            SetUser("abc");
            var dto = new SendMessageDto
            {
                ReceiverId = 2,
                Content = "Hello"
            };

            var result = await _controller.SendMessage(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid user ID", badRequest.Value);
        }

        [Fact]
        public async Task SendMessage_ReturnsUnauthorized_WhenServiceThrows()
        {
            SetUser();
            var dto = new SendMessageDto
            {
                ReceiverId = 2,
                Content = "Hello"
            };

            _mockChatService
                .Setup(s => s.SendMessageAsync(1, dto))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.SendMessage(dto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task GetMessages_ReturnsOkResult_WhenMessagesExist()
        {
            SetUser();
            var expected = new List<MessageResponseDto>
            {
                new MessageResponseDto
                {
                    Id = 1,
                    SenderId = 1,
                    Content = "Hi",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockChatService
                .Setup(s => s.GetMessagesAsync(1, 2))
                .ReturnsAsync(expected);

            var result = await _controller.GetMessages(2);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MessageResponseDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetMessages_ReturnsUnauthorized_WhenServiceThrows()
        {
            SetUser();
            _mockChatService
                .Setup(s => s.GetMessagesAsync(1, 2))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetMessages(2);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task GetMeetupMessages_ReturnsOkResult_WhenMessagesExist()
        {
            SetUser();
            var expected = new List<MessageResponseDto>
            {
                new MessageResponseDto
                {
                    Id = 1,
                    SenderId = 1,
                    Content = "Meetup message",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockChatService
                .Setup(s => s.GetMeetupMessagesAsync(1, 2, 10))
                .ReturnsAsync(expected);

            var result = await _controller.GetMeetupMessages(10, 2);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MessageResponseDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetMeetupMessages_ReturnsUnauthorized_WhenServiceThrows()
        {
            SetUser();
            _mockChatService
                .Setup(s => s.GetMeetupMessagesAsync(1, 2, 10))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetMeetupMessages(10, 2);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task GetConversations_ReturnsOkResult_WhenConversationsExist()
        {
            SetUser();
            var expected = new List<ConversationDto>
            {
                new ConversationDto
                {
                    ConversationId = 1,
                    MeetupEventId = 10,
                    OtherUserId = 2,
                    OtherUserName = "partner",
                    LastMessageAt = DateTime.UtcNow,
                    IsClosed = false,
                    IsExpired = false
                }
            };

            _mockChatService
                .Setup(s => s.GetConversationsAsync(1))
                .ReturnsAsync(expected);

            var result = await _controller.GetConversations();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<ConversationDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetConversations_ReturnsUnauthorized_WhenServiceThrows()
        {
            SetUser();
            _mockChatService
                .Setup(s => s.GetConversationsAsync(1))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetConversations();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
    }
}

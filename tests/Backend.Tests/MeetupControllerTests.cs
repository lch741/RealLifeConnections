using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO.Meetup;
using backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests
{
    public class MeetupControllerTests
    {
        private readonly Mock<IMeetupService> _mockMeetupService;
        private readonly MeetupController _controller;

        public MeetupControllerTests()
        {
            _mockMeetupService = new Mock<IMeetupService>();
            _controller = new MeetupController(_mockMeetupService.Object);
        }

        private void SetUser()
        {
            var userClaims = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };
        }

        #region Create Tests
        [Fact]
        public async Task Create_ReturnsOkResult_WhenCreateSucceeds()
        {
            SetUser();
            var dto = new CreateMeetupDto
            {
                Title = "Coffee meetup",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5
            };

            var expected = new MeetupEventDto
            {
                Id = 1,
                Title = dto.Title,
                Region = dto.Region,
                Suburb = dto.Suburb,
                ActivityId = dto.ActivityId,
                ActivityName = "Coffee",
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                MaxParticipants = dto.MaxParticipants,
                CreatorId = 1,
                CreatorName = "tester"
            };

            _mockMeetupService
                .Setup(s => s.CreateMeetupAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CreateMeetupDto>()))
                .ReturnsAsync(expected);

            var result = await _controller.Create(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<MeetupEventDto>(ok.Value);
            Assert.Equal(expected.Title, value.Title);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenCreateFails()
        {
            SetUser();
            var dto = new CreateMeetupDto
            {
                Title = "Coffee meetup",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5
            };

            _mockMeetupService
                .Setup(s => s.CreateMeetupAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CreateMeetupDto>()))
                .ThrowsAsync(new InvalidOperationException("Invalid meetup"));

            var result = await _controller.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid meetup", badRequest.Value);
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            var dto = new CreateMeetupDto
            {
                Title = "Coffee meetup",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5
            };

            _mockMeetupService
                .Setup(s => s.CreateMeetupAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CreateMeetupDto>()))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.Create(dto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion

        #region GetById Tests
        [Fact]
        public async Task GetById_ReturnsOkResult_WhenMeetupExists()
        {
            var expected = new MeetupEventDto
            {
                Id = 12,
                Title = "Test meetup",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                ActivityName = "Coffee",
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5,
                CreatorId = 1,
                CreatorName = "tester"
            };

            _mockMeetupService
                .Setup(s => s.GetMeetupAsync(12))
                .ReturnsAsync(expected);

            var result = await _controller.GetById(12);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<MeetupEventDto>(ok.Value);
            Assert.Equal(expected.Id, value.Id);
        }

        [Fact]
        public async Task GetById_ReturnsBadRequest_WhenMeetupMissing()
        {
            _mockMeetupService
                .Setup(s => s.GetMeetupAsync(12))
                .ThrowsAsync(new InvalidOperationException("Meetup not found"));

            var result = await _controller.GetById(12);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Meetup not found", badRequest.Value);
        }
        #endregion

        #region GetCreatedByMe Tests
        [Fact]
        public async Task GetCreatedByMe_ReturnsOkResult_WhenSuccessful()
        {
            SetUser();
            var expected = new List<MeetupEventDto>
            {
                new MeetupEventDto
                {
                    Id = 1,
                    Title = "My meetup",
                    Region = "TestRegion",
                    Suburb = "TestSuburb",
                    ActivityId = 1,
                    ActivityName = "Coffee",
                    EventDate = DateTime.UtcNow.Date.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0),
                    MaxParticipants = 5,
                    CreatorId = 1,
                    CreatorName = "tester"
                }
            };

            _mockMeetupService
                .Setup(s => s.GetUserCreatedMeetupsAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetCreatedByMe();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MeetupEventDto>>(ok.Value);
            Assert.Single(value);
        }

        [Fact]
        public async Task GetCreatedByMe_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.GetUserCreatedMeetupsAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetCreatedByMe();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion

        #region Update Tests
        [Fact]
        public async Task Update_ReturnsOkResult_WhenUpdateSucceeds()
        {
            SetUser();
            var dto = new UpdateMeetupDto
            {
                Title = "Updated title"
            };

            var expected = new MeetupEventDto
            {
                Id = 1,
                Title = "Updated title",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                ActivityName = "Coffee",
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5,
                CreatorId = 1,
                CreatorName = "tester"
            };

            _mockMeetupService
                .Setup(s => s.UpdateMeetupAsync(It.IsAny<ClaimsPrincipal>(), 1, It.IsAny<UpdateMeetupDto>()))
                .ReturnsAsync(expected);

            var result = await _controller.Update(1, dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<MeetupEventDto>(ok.Value);
            Assert.Equal("Updated title", value.Title);
        }

        [Fact]
        public async Task Update_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            var dto = new UpdateMeetupDto { Title = "Updated title" };

            _mockMeetupService
                .Setup(s => s.UpdateMeetupAsync(It.IsAny<ClaimsPrincipal>(), 1, It.IsAny<UpdateMeetupDto>()))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.Update(1, dto);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task Delete_ReturnsOkResult_WhenDeleteSucceeds()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.DeleteMeetupAsync(It.IsAny<ClaimsPrincipal>(), 1))
                .Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Delete_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.DeleteMeetupAsync(It.IsAny<ClaimsPrincipal>(), 1))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.Delete(1);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion

        #region UpdateStatus Tests
        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenStatusMissing()
        {
            var result = await _controller.UpdateStatus(1, " ");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Status is required.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOkResult_WhenUpdateSucceeds()
        {
            SetUser();
            var expected = new MeetupEventDto
            {
                Id = 1,
                Title = "Test meetup",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                ActivityId = 1,
                ActivityName = "Coffee",
                EventDate = DateTime.UtcNow.Date.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                MaxParticipants = 5,
                CreatorId = 1,
                CreatorName = "tester"
            };

            _mockMeetupService
                .Setup(s => s.UpdateMeetupStatusAsync(It.IsAny<ClaimsPrincipal>(), 1, "Open"))
                .ReturnsAsync(expected);

            var result = await _controller.UpdateStatus(1, "Open");

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<MeetupEventDto>(ok.Value);
            Assert.Equal(expected.Id, value.Id);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenUpdateFails()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.UpdateMeetupStatusAsync(It.IsAny<ClaimsPrincipal>(), 1, "Open"))
                .ThrowsAsync(new InvalidOperationException("Invalid status"));

            var result = await _controller.UpdateStatus(1, "Open");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid status", badRequest.Value);
        }
        #endregion
    }
}

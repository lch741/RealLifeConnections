using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO;
using backend.DTO.Meetup;
using backend.DTO.Matching;
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
                Activities = new List<ActivityInputDto>
                {
                    new ActivityInputDto { Name = "Coffee", Order = 1 }
                },
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
                Activities = new List<ActivityDto>
                {
                    new ActivityDto { Id = 1, Name = "Coffee" }
                },
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
                Activities = new List<ActivityInputDto>
                {
                    new ActivityInputDto { Name = "Coffee", Order = 1 }
                },
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
                Activities = new List<ActivityInputDto>
                {
                    new ActivityInputDto { Name = "Coffee", Order = 1 }
                },
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
                Activities = new List<ActivityDto>
                {
                    new ActivityDto { Id = 1, Name = "Coffee" }
                },
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
                    Activities = new List<ActivityDto>
                    {
                        new ActivityDto { Id = 1, Name = "Coffee" }
                    },
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

        #region GetJoinedByMe Tests
        [Fact]
        public async Task GetJoinedByMe_ReturnsOkResult_WhenSuccessful()
        {
            SetUser();
            var expected = new List<MeetupEventDto>
            {
                new MeetupEventDto
                {
                    Id = 2,
                    Title = "Joined meetup",
                    Region = "TestRegion",
                    Suburb = "TestSuburb",
                    Activities = new List<ActivityDto>
                    {
                        new ActivityDto { Id = 1, Name = "Coffee" }
                    },
                    EventDate = DateTime.UtcNow.Date.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0),
                    MaxParticipants = 5,
                    CreatorId = 1,
                    CreatorName = "tester"
                }
            };

            _mockMeetupService
                .Setup(s => s.GetUserJoinedMeetupsAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetJoinedByMe();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MeetupEventDto>>(ok.Value);
            Assert.Single(value);
            Assert.Equal(2, value[0].Id);
        }

        [Fact]
        public async Task GetJoinedByMe_ReturnsBadRequest_WhenServiceThrows()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.GetUserJoinedMeetupsAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new InvalidOperationException("Invalid request"));

            var result = await _controller.GetJoinedByMe();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid request", badRequest.Value);
        }

        [Fact]
        public async Task GetJoinedByMe_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.GetUserJoinedMeetupsAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetJoinedByMe();

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
                Activities = new List<ActivityDto>
                {
                    new ActivityDto { Id = 1, Name = "Coffee" }
                },
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
                Activities = new List<ActivityDto>
                {
                    new ActivityDto { Id = 1, Name = "Coffee" }
                },
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

        [Fact]
        public async Task UpdateStatus_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.UpdateMeetupStatusAsync(It.IsAny<ClaimsPrincipal>(), 1, "Open"))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.UpdateStatus(1, "Open");

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion

        #region Join/Quit/Approve Tests
        [Fact]
        public async Task Apply_ReturnsOkResult_WhenJoinSucceeds()
        {
            SetUser();
            var expected = new UserMeetupDto
            {
                Id = 10,
                UserId = 1,
                UserName = "tester",
                Status = "Pending",
                JoinedAt = DateTime.UtcNow
            };

            _mockMeetupService
                .Setup(s => s.JoinMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .ReturnsAsync(expected);

            var result = await _controller.Apply(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserMeetupDto>(ok.Value);
            Assert.Equal(expected.Id, value.Id);
        }

        [Fact]
        public async Task Apply_ReturnsBadRequest_WhenJoinFails()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.JoinMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .ThrowsAsync(new InvalidOperationException("Meetup is full."));

            var result = await _controller.Apply(5);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Meetup is full.", badRequest.Value);
        }

        [Fact]
        public async Task Apply_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.JoinMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.Apply(5);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task Quit_ReturnsOkResult_WhenLeaveSucceeds()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.LeaveMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .Returns(Task.CompletedTask);

            var result = await _controller.Quit(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Quit_ReturnsBadRequest_WhenLeaveFails()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.LeaveMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .ThrowsAsync(new InvalidOperationException("You have not joined this meetup."));

            var result = await _controller.Quit(5);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("You have not joined this meetup.", badRequest.Value);
        }

        [Fact]
        public async Task Quit_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.LeaveMeetupAsync(It.IsAny<ClaimsPrincipal>(), 5))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.Quit(5);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task ApproveParticipant_ReturnsOkResult_WhenApproveSucceeds()
        {
            SetUser();
            var expected = new UserMeetupDto
            {
                Id = 11,
                UserId = 22,
                UserName = "participant",
                Status = "Approved",
                JoinedAt = DateTime.UtcNow,
                IsConfirmed = true,
                ConfirmedAt = DateTime.UtcNow
            };

            _mockMeetupService
                .Setup(s => s.ConfirmParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .ReturnsAsync(expected);

            var result = await _controller.ApproveParticipant(5, 22);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserMeetupDto>(ok.Value);
            Assert.Equal(expected.UserId, value.UserId);
            Assert.True(value.IsConfirmed);
        }

        [Fact]
        public async Task ApproveParticipant_ReturnsBadRequest_WhenApproveFails()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.ConfirmParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .ThrowsAsync(new InvalidOperationException("Participant not found."));

            var result = await _controller.ApproveParticipant(5, 22);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Participant not found.", badRequest.Value);
        }

        [Fact]
        public async Task ApproveParticipant_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.ConfirmParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.ApproveParticipant(5, 22);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        [Fact]
        public async Task RejectParticipant_ReturnsOkResult_WhenRejectSucceeds()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.RejectParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .Returns(Task.CompletedTask);

            var result = await _controller.RejectParticipant(5, 22);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task RejectParticipant_ReturnsBadRequest_WhenRejectFails()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.RejectParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .ThrowsAsync(new InvalidOperationException("Participant not found."));

            var result = await _controller.RejectParticipant(5, 22);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Participant not found.", badRequest.Value);
        }

        [Fact]
        public async Task RejectParticipant_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.RejectParticipantAsync(It.IsAny<ClaimsPrincipal>(), 5, 22))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.RejectParticipant(5, 22);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }

        #endregion

        
        #region GetMatchedMeetups Tests
        [Fact]
        public async Task GetMatchedMeetups_ReturnsOkResult_WhenSuccessful()
        {
            SetUser();
            var expected = new List<MeetupMatchDto>
            {
                new MeetupMatchDto
                {
                    MeetupId = 1,
                    Title = "Matched meetup",
                    Region = "TestRegion",
                    Suburb = "TestSuburb",
                    ActivityName = "Coffee",
                    EventDate = DateTime.UtcNow.Date.AddDays(2),
                    StartTime = new TimeSpan(10, 0, 0),
                    MaxParticipants = 5,
                    CurrentParticipants = 1,
                    Status = "Open",
                    MatchScore = 85,
                    CreatorId = 2,
                    CreatorName = "creator"
                }
            };

            _mockMeetupService
                .Setup(s => s.GetMatchedMeetupsAsync(It.IsAny<ClaimsPrincipal>(), "Cafe", "TestSuburb", 20))
                .ReturnsAsync(expected);

            var result = await _controller.GetMatchedMeetups("Cafe", "TestSuburb", 20);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<MeetupMatchDto>>(ok.Value);
            Assert.Single(value);
            Assert.Equal(85, value[0].MatchScore);
        }

        [Fact]
        public async Task GetMatchedMeetups_ReturnsBadRequest_WhenServiceThrows()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.GetMatchedMeetupsAsync(It.IsAny<ClaimsPrincipal>(), "Cafe", "TestSuburb", 20))
                .ThrowsAsync(new InvalidOperationException("Invalid activity type."));

            var result = await _controller.GetMatchedMeetups("Cafe", "TestSuburb", 20);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type.", badRequest.Value);
        }

        [Fact]
        public async Task GetMatchedMeetups_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            SetUser();
            _mockMeetupService
                .Setup(s => s.GetMatchedMeetupsAsync(It.IsAny<ClaimsPrincipal>(), "Cafe", "TestSuburb", 20))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            var result = await _controller.GetMatchedMeetups("Cafe", "TestSuburb", 20);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authorized", unauthorized.Value);
        }
        #endregion


    }
}

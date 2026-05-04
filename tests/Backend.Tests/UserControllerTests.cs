using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using Xunit;
using backend.Controllers;
using backend.Interfaces;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Backend.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        #region Register Tests
        [Fact]
        public async Task Register_ReturnsOkResult_WhenRegistrationSucceeds()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "test@example.com",
                UserName = "tester",
                Password = "Password123!!",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                InterestSelections = new List<RegisterInterestSelectionDto>
                {
                    new RegisterInterestSelectionDto { CategoryId = 1, Interests = new List<string>{ "Coffee" } }
                }
            };

            var expected = new AuthResponseDto
            {
                Message = "Registration successful",
                Token = "FAKE_TOKEN",
                User = new AuthUserDto
                {
                    Email = registerDto.Email,
                    UserName = registerDto.UserName,
                    Bio = null,
                    Region = registerDto.Region,
                    Suburb = registerDto.Suburb,
                }
            };

            _mockUserService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserDto>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<AuthResponseDto>(ok.Value);
            Assert.Equal(expected.Message, value.Message);
            Assert.Equal(expected.Token, value.Token);
            Assert.Equal(expected.User.Email, value.User.Email);
            Assert.Equal(expected.User.UserName, value.User.UserName);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "test@example.com",
                UserName = "tester",
                Password = "Password123!!",
                Region = "TestRegion",
                Suburb = "TestSuburb",
                InterestSelections = new List<RegisterInterestSelectionDto>()
            };

            _mockUserService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserDto>()))
                .ThrowsAsync(new InvalidOperationException("Email already exists"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already exists", badRequest.Value);
        }

        [Fact]
        public async Task Register_ReturnsInternalServerError_OnUnexpectedException()
        {
            // Arrange
            var registerDto = new RegisterUserDto { 
                Email = "test@example.com",
                UserName = "tester",
                Password = "Password123!!",
                Region = "TestRegion",
                Suburb = "TestSuburb"
            };

            _mockUserService
                .Setup(s => s.RegisterAsync(It.IsAny<RegisterUserDto>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var statusCode = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCode.StatusCode);
        }
        #endregion

        #region Login Tests
        [Fact]
        public async Task Login_ReturnsOkResult_WhenLoginSucceeds()
        {
            // Arrange
            var loginDto = new LoginUserDto
            {
                Email = "test@example.com",
                Password = "Password123!!"
            };

            var expected = new AuthResponseDto
            {
                Message = "Login successful",
                Token = "JWT_TOKEN",
                User = new AuthUserDto
                {
                    Email = loginDto.Email,
                    UserName = "tester",
                    Region = "TestRegion",
                    Suburb = "TestSuburb"
                }
            };

            _mockUserService
                .Setup(s => s.LoginAsync(It.IsAny<LoginUserDto>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<AuthResponseDto>(ok.Value);
            Assert.Equal(expected.Message, value.Message);
            Assert.Equal(expected.Token, value.Token);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
        {
            // Arrange
            var loginDto = new LoginUserDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _mockUserService
                .Setup(s => s.LoginAsync(It.IsAny<LoginUserDto>()))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid credentials", unauthorized.Value);
        }

        [Fact]
        public async Task Login_ReturnsInternalServerError_OnUnexpectedException()
        {
            // Arrange
            var loginDto = new LoginUserDto { Email = "test@example.com", Password = "Password123!!" };

            _mockUserService
                .Setup(s => s.LoginAsync(It.IsAny<LoginUserDto>()))
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var statusCode = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCode.StatusCode);
        }
        #endregion

        #region GetProfile Tests
        [Fact]
        public async Task GetProfile_ReturnsOkResult_WhenSuccessful()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var expected = new UserProfileDto
            {
                Id = 1,
                UserName = "tester",
                Email = "test@example.com",
                Bio = "Test bio",
                Region = "TestRegion",
                Suburb = "TestSuburb"
            };

            _mockUserService
                .Setup(s => s.GetProfileAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserProfileDto>(ok.Value);
            Assert.Equal(expected.Id, value.Id);
            Assert.Equal(expected.UserName, value.UserName);
        }

        [Fact]
        public async Task GetProfile_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal();
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.GetProfileAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new UnauthorizedAccessException("User not authenticated"));

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated", unauthorized.Value);
        }
        #endregion

        #region UpdateProfile Tests
        [Fact]
        public async Task UpdateProfile_ReturnsOkResult_WhenUpdateSucceeds()
        {
            // Arrange
            var updateDto = new UpdateProfileDto
            {
                Bio = "Updated bio",
                Region = "NewRegion",
                Suburb = "NewSuburb"
            };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var expected = new UserProfileDto
            {
                Id = 1,
                UserName = "tester",
                Email = "test@example.com",
                Bio = updateDto.Bio,
                Region = updateDto.Region,
                Suburb = updateDto.Suburb
            };

            _mockUserService
                .Setup(s => s.UpdateProfileAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<UpdateProfileDto>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.UpdateProfile(updateDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserProfileDto>(ok.Value);
            Assert.Equal(expected.Bio, value.Bio);
            Assert.Equal(expected.Region, value.Region);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsBadRequest_WhenInvalidData()
        {
            // Arrange
            var updateDto = new UpdateProfileDto { Bio = "" };

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.UpdateProfileAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<UpdateProfileDto>()))
                .ThrowsAsync(new InvalidOperationException("Bio cannot be empty"));

            // Act
            var result = await _controller.UpdateProfile(updateDto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Bio cannot be empty", badRequest.Value);
        }

        [Fact]
        public async Task UpdateProfile_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var updateDto = new UpdateProfileDto { Bio = "Updated bio" };

            var userClaims = new ClaimsPrincipal();
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.UpdateProfileAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<UpdateProfileDto>()))
                .ThrowsAsync(new UnauthorizedAccessException("User not authenticated"));

            // Act
            var result = await _controller.UpdateProfile(updateDto);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated", unauthorized.Value);
        }
        #endregion

        #region SaveAvatarUpload Tests
        [Fact]
        public async Task SaveAvatarUpload_ReturnsOkResult_WhenUploadSucceeds()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("avatar.jpg");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var expected = new UserProfileDto
            {
                Id = 1,
                UserName = "tester",
                Email = "test@example.com",
                AvatarUrl = "https://example.com/uploads/avatar.jpg",
                Region = "TestRegion",
                Suburb = "TestSuburb"
            };

            _mockUserService
                .Setup(s => s.SaveAvatarAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.SaveAvatarUpload(mockFile.Object);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserProfileDto>(ok.Value);
            Assert.Equal(expected.AvatarUrl, value.AvatarUrl);
        }

        [Fact]
        public async Task SaveAvatarUpload_ReturnsBadRequest_WhenInvalidFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("avatar.txt");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.SaveAvatarAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ThrowsAsync(new InvalidOperationException("Invalid file format"));

            // Act
            var result = await _controller.SaveAvatarUpload(mockFile.Object);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid file format", badRequest.Value);
        }

        [Fact]
        public async Task SaveAvatarUpload_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("avatar.jpg");

            var userClaims = new ClaimsPrincipal();
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.SaveAvatarAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ThrowsAsync(new UnauthorizedAccessException("User not authenticated"));

            // Act
            var result = await _controller.SaveAvatarUpload(mockFile.Object);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated", unauthorized.Value);
        }
        #endregion

        #region VerifyFaceUpload Tests
        [Fact]
        public async Task VerifyFaceUpload_ReturnsOkResult_WhenUploadSucceeds()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("livecapture.jpg");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            var expected = new FaceVerificationResponseDto
            {
                Status = "success",
                Message = "Face verified from upload",
                IsVerified = true,
                Confidence = 0.92
            };

            _mockUserService
                .Setup(s => s.VerifyFaceAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.VerifyFaceUpload(mockFile.Object);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<FaceVerificationResponseDto>(ok.Value);
            Assert.True(value.IsVerified);
        }

        [Fact]
        public async Task VerifyFaceUpload_ReturnsBadRequest_WhenInvalidFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.pdf");

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.VerifyFaceAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ThrowsAsync(new InvalidOperationException("File must be an image"));

            // Act
            var result = await _controller.VerifyFaceUpload(mockFile.Object);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("File must be an image", badRequest.Value);
        }

        [Fact]
        public async Task VerifyFaceUpload_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("livecapture.jpg");

            var userClaims = new ClaimsPrincipal();
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = userClaims } };

            _mockUserService
                .Setup(s => s.VerifyFaceAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<IFormFile>()))
                .ThrowsAsync(new UnauthorizedAccessException("User not authenticated"));

            // Act
            var result = await _controller.VerifyFaceUpload(mockFile.Object);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("User not authenticated", unauthorized.Value);
        }
        #endregion
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SpotifyTrendsApp.Server.Controllers;
using SpotifyTrendsApp.Server.Services;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Tests
{
    public class LoginControllerTests
    {
        private readonly Mock<ILogger<LoginController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly LoginController _controller;

        public LoginControllerTests()
        {
            _mockLogger = new Mock<ILogger<LoginController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenService = new Mock<ITokenService>();

            // Setup JWT configuration with proper mock for GetValue
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Issuer"]).Returns("SpotifyTrendsApp");
            jwtSection.Setup(x => x["Audience"]).Returns("SpotifyTrendsUsers");
            jwtSection.Setup(x => x["ExpiresInMinutes"]).Returns("60");
            
            // Mock the GetValue extension method by setting up a custom configuration
            var configData = new Dictionary<string, string>
            {
                {"Jwt:ExpiresInMinutes", "60"},
                {"Jwt:Issuer", "SpotifyTrendsApp"},
                {"Jwt:Audience", "SpotifyTrendsUsers"},
                {"Spotify:ClientId", "test_client_id"},
                {"Spotify:RedirectUri", "http://localhost:5000/callback"},
                {"Spotify:Scopes", "user-top-read"},
                {"ClientApp:BaseUrl", "http://localhost:5173"}
            };
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData!)
                .Build();

            _controller = new LoginController(_mockLogger.Object, configuration, _mockTokenService.Object);
        }

        [Fact]
        public void Connect_WithValidConfiguration_ReturnsRedirectResult()
        {
            // Act
            var result = _controller.Connect();

            // Assert
            Assert.IsType<RedirectResult>(result);
            var redirectResult = result as RedirectResult;
            Assert.NotNull(redirectResult);
            Assert.Contains("https://accounts.spotify.com/authorize", redirectResult!.Url);
            Assert.Contains("client_id=test_client_id", redirectResult.Url);
        }

        [Fact]
        public void Connect_WithMissingClientId_ReturnsBadRequest()
        {
            // Arrange - Create a new controller with a fresh configuration that has no ClientId
            var mockConfigWithNoClientId = new Mock<IConfiguration>();
            mockConfigWithNoClientId.Setup(c => c["Spotify:ClientId"]).Returns((string?)null);
            mockConfigWithNoClientId.Setup(c => c["Spotify:RedirectUri"]).Returns((string?)null);
            mockConfigWithNoClientId.Setup(c => c["Spotify:Scopes"]).Returns("user-top-read");
            
            var controllerWithNoConfig = new LoginController(_mockLogger.Object, mockConfigWithNoClientId.Object, _mockTokenService.Object);
            
            // Store and clear environment variables
            var originalClientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
            var originalRedirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI");
            
            try
            {
                Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_ID", null);
                Environment.SetEnvironmentVariable("SPOTIFY_REDIRECT_URI", null);

                // Act
                var result = controllerWithNoConfig.Connect();

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
                var badRequestResult = result as BadRequestObjectResult;
                Assert.Equal("Spotify ClientId is not configured", badRequestResult?.Value);
            }
            finally
            {
                // Restore original values
                Environment.SetEnvironmentVariable("SPOTIFY_CLIENT_ID", originalClientId);
                Environment.SetEnvironmentVariable("SPOTIFY_REDIRECT_URI", originalRedirectUri);
            }
        }

        [Fact]
        public async Task Callback_WithMissingCode_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Callback(string.Empty, "some_state");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Missing code", badRequestResult?.Value);
        }

        [Fact]
        public async Task Callback_WithValidCode_ReturnsRedirect()
        {
            // Arrange
            var tokenInfo = new TokenInfo
            {
                AccessToken = "access_token_123",
                RefreshToken = "refresh_token_123",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Set up environment variable for JWT key
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "test_jwt_secret_key_that_is_32_chars");

            _mockTokenService.Setup(s => s.GetAccessTokenAsync(It.IsAny<string>()))
                .ReturnsAsync(tokenInfo);

            // Act
            var result = await _controller.Callback("valid_code", "some_state");

            // Assert
            Assert.IsType<RedirectResult>(result);
            var redirectResult = result as RedirectResult;
            Assert.NotNull(redirectResult);
            Assert.Contains("http://localhost:5173", redirectResult!.Url);
            Assert.Contains("token=", redirectResult.Url);

            // Clean up
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        }
    }
}

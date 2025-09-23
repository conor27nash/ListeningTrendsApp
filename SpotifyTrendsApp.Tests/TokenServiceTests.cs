using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SpotifyTrendsApp.Server.Models;
using SpotifyTrendsApp.Server.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SpotifyTrendsApp.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<TokenService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<TokenService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            // Setup configuration
            _mockConfiguration.Setup(c => c["Spotify:ClientId"]).Returns("test_client_id");
            _mockConfiguration.Setup(c => c["Spotify:ClientSecret"]).Returns("test_client_secret");
            _mockConfiguration.Setup(c => c["Spotify:RedirectUri"]).Returns("http://localhost:5000/callback");
            _mockConfiguration.Setup(c => c["Spotify:TokenEndpoint"]).Returns("https://accounts.spotify.com/api/token");

            _tokenService = new TokenService(_httpClient, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_WithValidCode_ReturnsTokenInfo()
        {
            // Arrange
            var code = "valid_auth_code";
            var expectedResponse = new
            {
                access_token = "access_token_123",
                refresh_token = "refresh_token_123",
                expires_in = 3600
            };

            var responseContent = JsonSerializer.Serialize(expectedResponse);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _tokenService.GetAccessTokenAsync(code);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access_token_123", result.AccessToken);
            Assert.Equal("refresh_token_123", result.RefreshToken);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task GetAccessTokenAsync_WithInvalidCode_ThrowsHttpRequestException()
        {
            // Arrange
            var invalidCode = "invalid_code";
            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("invalid_grant", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _tokenService.GetAccessTokenAsync(invalidCode));
        }

        [Fact]
        public async Task GetAccessTokenAsync_WithMissingConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockConfigWithMissingValues = new Mock<IConfiguration>();
            mockConfigWithMissingValues.Setup(c => c["Spotify:ClientId"]).Returns((string?)null);
            mockConfigWithMissingValues.Setup(c => c["Spotify:ClientSecret"]).Returns("secret");
            mockConfigWithMissingValues.Setup(c => c["Spotify:RedirectUri"]).Returns("uri");
            mockConfigWithMissingValues.Setup(c => c["Spotify:TokenEndpoint"]).Returns("endpoint");
            
            var serviceWithMissingConfig = new TokenService(_httpClient, mockConfigWithMissingValues.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                serviceWithMissingConfig.GetAccessTokenAsync("test_code"));
        }
    }
}
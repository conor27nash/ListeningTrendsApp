using System;
using Xunit;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Tests.Models
{
    public class TokenInfoTests
    {
        [Fact]
        public void TokenInfo_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var tokenInfo = new TokenInfo
            {
                AccessToken = "test-access-token",
                RefreshToken = "test-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Assert
            Assert.NotNull(tokenInfo.AccessToken);
            Assert.NotNull(tokenInfo.RefreshToken);
            Assert.True(tokenInfo.ExpiresAt > DateTime.UtcNow);
        }
    }
}
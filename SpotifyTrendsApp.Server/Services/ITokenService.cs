using Microsoft.AspNetCore.Authentication.OAuth;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Server.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Gets an access token using the authorization code
        /// </summary>
        /// <param name="code">The authorization code received from Spotify</param>
        /// <returns>OAuth token response containing access and refresh tokens</returns>
        Task<TokenInfo> GetAccessTokenAsync(string code);

        /// <summary>
        /// Refreshes the access token using a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token to use</param>
        /// <returns>OAuth token response containing new access token</returns>
        Task<TokenInfo> RefreshAccessTokenAsync(string refreshToken);
        
        /// <summary>
        /// Gets the stored refresh token
        /// </summary>
        /// <returns>The stored refresh token, or null if not available</returns>
        Task<string?> GetStoredRefreshTokenAsync();
        
        /// <summary>
        /// Updates the stored refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token to store</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<string?> GetStoredAccessTokenAsync();
           TokenInfo CurrentToken { get; set; }
    }
}
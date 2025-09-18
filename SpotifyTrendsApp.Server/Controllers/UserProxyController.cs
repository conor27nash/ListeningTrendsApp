using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SpotifyTrendsApp.Server.Services;
using System.Text.Json;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserProxyController> _logger;

        public UserProxyController(
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            ILogger<UserProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;
        }

        private string? GetSpotifyAccessToken()
        {
            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found in JWT");
                return null;
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);
            return tokenInfo;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                var client = _httpClientFactory.CreateClient("UserService");
                var request = new HttpRequestMessage(HttpMethod.Get, "api/user/profile");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("UserService GetUserProfile failed: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                var userProfile = JsonSerializer.Deserialize<object>(content);
                
                _logger.LogInformation("Successfully retrieved user profile");
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserProfile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("following/artists")]
        public async Task<IActionResult> GetFollowedArtists(
            [FromQuery] string? after = null,
            [FromQuery] int limit = 20)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                var client = _httpClientFactory.CreateClient("UserService");
                
                var queryParams = new List<string> { $"limit={limit}" };
                if (!string.IsNullOrEmpty(after)) queryParams.Add($"after={after}");
                var queryString = string.Join("&", queryParams);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/user/following/artists?{queryString}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("UserService GetFollowedArtists failed: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var content = await response.Content.ReadAsStringAsync();
                var followedArtists = JsonSerializer.Deserialize<object>(content);
                
                _logger.LogInformation("Successfully retrieved followed artists");
                return Ok(followedArtists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFollowedArtists");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

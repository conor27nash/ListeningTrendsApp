using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserController> _logger;

        public UserController(HttpClient httpClient, ILogger<UserController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile(
            [FromHeader] string Authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetUserProfile called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                var url = "https://api.spotify.com/v1/me";

                _logger.LogInformation("Fetching current user profile");

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch user profile: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var userProfile = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched user profile");
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("following/artists")]
        public async Task<IActionResult> GetFollowedArtists(
            [FromHeader] string Authorization,
            [FromQuery] string? after = null,
            [FromQuery] int limit = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetFollowedArtists called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                limit = Math.Min(Math.Max(limit, 1), 50);

                var queryParams = new List<string> { $"type=artist", $"limit={limit}" };
                if (!string.IsNullOrEmpty(after)) queryParams.Add($"after={after}");

                var queryString = string.Join("&", queryParams);
                var url = $"https://api.spotify.com/v1/me/following?{queryString}";

                _logger.LogInformation("Fetching followed artists with limit: {Limit}", limit);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch followed artists: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var followedArtists = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched followed artists");
                return Ok(followedArtists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching followed artists");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

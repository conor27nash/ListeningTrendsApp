using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyTrendsApp.Server.Services;
using System.Security.Claims;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TracksProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TracksProxyController> _logger;

        public TracksProxyController(
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            ILogger<TracksProxyController> logger)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrack(string id, [FromQuery] string? market = null)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tracks/{id}{queryString}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get track: {TrackId}", id);
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get track request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("several")]
        public async Task<IActionResult> GetSeveralTracks([FromQuery] string ids, [FromQuery] string? market = null)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var queryParams = new List<string> { $"ids={ids}" };
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                var queryString = string.Join("&", queryParams);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tracks/several?{queryString}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get several tracks");
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get several tracks request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedTracks([FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string? market = null)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var queryParams = new List<string> { $"limit={limit}", $"offset={offset}" };
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                var queryString = string.Join("&", queryParams);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tracks/saved?{queryString}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get saved tracks");
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get saved tracks request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("save")]
        public async Task<IActionResult> SaveTracks([FromBody] SaveTracksRequest request)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var requestMessage = new HttpRequestMessage(HttpMethod.Put, "/api/tracks/save");
                requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
                
                var json = System.Text.Json.JsonSerializer.Serialize(request);
                requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Proxying request to save tracks");
                var response = await client.SendAsync(requestMessage);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying save tracks request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveSavedTracks([FromBody] SaveTracksRequest request)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/tracks/remove");
                requestMessage.Headers.Add("Authorization", $"Bearer {accessToken}");
                
                var json = System.Text.Json.JsonSerializer.Serialize(request);
                requestMessage.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Proxying request to remove saved tracks");
                var response = await client.SendAsync(requestMessage);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying remove saved tracks request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check-saved")]
        public async Task<IActionResult> CheckSavedTracks([FromQuery] string ids)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("TrackService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/tracks/check-saved?ids={ids}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to check saved tracks");
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying check saved tracks request");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class SaveTracksRequest
    {
        public List<string> Ids { get; set; } = new();
    }
}

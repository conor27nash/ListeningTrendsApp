using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyTrendsApp.Server.Services;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArtistsProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ArtistsProxyController> _logger;

        public ArtistsProxyController(
            IHttpClientFactory httpClientFactory,
            ITokenService tokenService,
            ILogger<ArtistsProxyController> logger)
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
        public async Task<IActionResult> GetArtist(string id)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("ArtistService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/artists/{id}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get artist: {ArtistId}", id);
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get artist request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("several")]
        public async Task<IActionResult> GetSeveralArtists([FromQuery] string ids)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("ArtistService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/artists/several?ids={ids}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get several artists");
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get several artists request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/top-tracks")]
        public async Task<IActionResult> GetArtistTopTracks(string id, [FromQuery] string market = "US")
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("ArtistService");
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/artists/{id}/top-tracks?market={market}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get artist top tracks: {ArtistId}", id);
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get artist top tracks request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/albums")]
        public async Task<IActionResult> GetArtistAlbums(
            string id, 
            [FromQuery] string? include_groups = null,
            [FromQuery] string? market = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                var accessToken = GetSpotifyAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Spotify access token not found");
                }

                using var client = _httpClientFactory.CreateClient("ArtistService");
                
                var queryParams = new List<string> { $"limit={limit}", $"offset={offset}" };
                if (!string.IsNullOrEmpty(include_groups)) queryParams.Add($"include_groups={include_groups}");
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                var queryString = string.Join("&", queryParams);
                
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/artists/{id}/albums?{queryString}");
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                _logger.LogInformation("Proxying request to get artist albums: {ArtistId}", id);
                var response = await client.SendAsync(request);
                
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying get artist albums request");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

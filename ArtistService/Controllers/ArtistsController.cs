using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace ArtistService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ArtistsController> _logger;

        public ArtistsController(HttpClient httpClient, ILogger<ArtistsController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET /api/artists/{id} - Get a single artist
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtist(
            string id, 
            [FromHeader] string Authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetArtist called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("GetArtist called with empty artist ID");
                    return BadRequest("Artist ID is required.");
                }

                var url = $"https://api.spotify.com/v1/artists/{id}";

                _logger.LogInformation("Fetching artist with ID: {ArtistId}", id);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch artist {ArtistId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var artist = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched artist {ArtistId}", id);
                return Ok(artist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching artist {ArtistId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/artists/several - Get several artists
        [HttpGet("several")]
        public async Task<IActionResult> GetSeveralArtists(
            [FromHeader] string Authorization,
            [FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetSeveralArtists called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(ids))
                {
                    _logger.LogWarning("GetSeveralArtists called with empty IDs");
                    return BadRequest("Artist IDs are required.");
                }

                // Validate artist IDs count (max 50)
                var artistIds = ids.Split(',');
                if (artistIds.Length > 50)
                {
                    _logger.LogWarning("GetSeveralArtists called with too many IDs: {Count}", artistIds.Length);
                    return BadRequest("Maximum 50 artist IDs allowed.");
                }

                var url = $"https://api.spotify.com/v1/artists?ids={ids}";

                _logger.LogInformation("Fetching {Count} artists", artistIds.Length);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch several artists: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var artists = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched {Count} artists", artistIds.Length);
                return Ok(artists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching several artists");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/artists/{id}/top-tracks - Get an artist's top tracks
        [HttpGet("{id}/top-tracks")]
        public async Task<IActionResult> GetArtistTopTracks(
            string id,
            [FromHeader] string Authorization,
            [FromQuery] string market = "US")
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetArtistTopTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("GetArtistTopTracks called with empty artist ID");
                    return BadRequest("Artist ID is required.");
                }

                var url = $"https://api.spotify.com/v1/artists/{id}/top-tracks?market={market}";

                _logger.LogInformation("Fetching top tracks for artist: {ArtistId} in market: {Market}", id, market);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch top tracks for artist {ArtistId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var topTracks = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched top tracks for artist {ArtistId}", id);
                return Ok(topTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top tracks for artist {ArtistId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/artists/{id}/albums - Get an artist's albums
        [HttpGet("{id}/albums")]
        public async Task<IActionResult> GetArtistAlbums(
            string id,
            [FromHeader] string Authorization,
            [FromQuery] string? include_groups = null,
            [FromQuery] string? market = null,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetArtistAlbums called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("GetArtistAlbums called with empty artist ID");
                    return BadRequest("Artist ID is required.");
                }

                // Validate and clamp parameters
                limit = Math.Min(Math.Max(limit, 1), 50);
                offset = Math.Max(offset, 0);

                var queryParams = new List<string> 
                { 
                    $"limit={limit}", 
                    $"offset={offset}" 
                };
                
                if (!string.IsNullOrEmpty(include_groups)) 
                    queryParams.Add($"include_groups={include_groups}");
                if (!string.IsNullOrEmpty(market)) 
                    queryParams.Add($"market={market}");

                var queryString = string.Join("&", queryParams);
                var url = $"https://api.spotify.com/v1/artists/{id}/albums?{queryString}";

                _logger.LogInformation("Fetching albums for artist: {ArtistId} with limit: {Limit}, offset: {Offset}", id, limit, offset);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch albums for artist {ArtistId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var albums = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched albums for artist {ArtistId}", id);
                return Ok(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching albums for artist {ArtistId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

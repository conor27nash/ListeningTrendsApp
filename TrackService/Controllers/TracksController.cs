using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace TrackService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TracksController> _logger;

        public TracksController(HttpClient httpClient, ILogger<TracksController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET /api/tracks/{id} - Get a single track
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrack(
            string id, 
            [FromHeader] string Authorization,
            [FromQuery] string? market = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetTrack called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogWarning("GetTrack called with empty track ID");
                    return BadRequest("Track ID is required.");
                }

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"https://api.spotify.com/v1/tracks/{id}{queryString}";

                _logger.LogInformation("Fetching track with ID: {TrackId}", id);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch track {TrackId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var track = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched track {TrackId}", id);
                return Ok(track);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching track {TrackId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/tracks/several - Get several tracks
        [HttpGet("several")]
        public async Task<IActionResult> GetSeveralTracks(
            [FromHeader] string Authorization,
            [FromQuery] string ids,
            [FromQuery] string? market = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetSeveralTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(ids))
                {
                    _logger.LogWarning("GetSeveralTracks called with empty IDs");
                    return BadRequest("Track IDs are required.");
                }

                // Validate track IDs count (max 50)
                var trackIds = ids.Split(',');
                if (trackIds.Length > 50)
                {
                    _logger.LogWarning("GetSeveralTracks called with too many IDs: {Count}", trackIds.Length);
                    return BadRequest("Maximum 50 track IDs allowed.");
                }

                var queryParams = new List<string> { $"ids={ids}" };
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                
                var queryString = string.Join("&", queryParams);
                var url = $"https://api.spotify.com/v1/tracks?{queryString}";

                _logger.LogInformation("Fetching {Count} tracks", trackIds.Length);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch several tracks: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var tracks = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched {Count} tracks", trackIds.Length);
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching several tracks");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/tracks/saved - Get user's saved tracks
        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedTracks(
            [FromHeader] string Authorization,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0,
            [FromQuery] string? market = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("GetSavedTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                // Validate and clamp parameters
                limit = Math.Min(Math.Max(limit, 1), 50);
                offset = Math.Max(offset, 0);

                var queryParams = new List<string> 
                { 
                    $"limit={limit}", 
                    $"offset={offset}" 
                };
                if (!string.IsNullOrEmpty(market)) queryParams.Add($"market={market}");
                
                var queryString = string.Join("&", queryParams);
                var url = $"https://api.spotify.com/v1/me/tracks?{queryString}";

                _logger.LogInformation("Fetching saved tracks with limit: {Limit}, offset: {Offset}", limit, offset);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch saved tracks: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var savedTracks = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully fetched saved tracks");
                return Ok(savedTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching saved tracks");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT /api/tracks/save - Save tracks for user
        [HttpPut("save")]
        public async Task<IActionResult> SaveTracks(
            [FromHeader] string Authorization,
            [FromBody] SaveTracksRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("SaveTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (request?.Ids == null || request.Ids.Count == 0)
                {
                    _logger.LogWarning("SaveTracks called with empty track IDs");
                    return BadRequest("Track IDs are required.");
                }

                if (request.Ids.Count > 50)
                {
                    _logger.LogWarning("SaveTracks called with too many IDs: {Count}", request.Ids.Count);
                    return BadRequest("Maximum 50 track IDs allowed.");
                }

                var url = "https://api.spotify.com/v1/me/tracks";
                var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
                requestMessage.Headers.Add("Authorization", Authorization);

                var requestBody = new { ids = request.Ids };
                var json = JsonSerializer.Serialize(requestBody);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Saving {Count} tracks", request.Ids.Count);

                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to save tracks: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                _logger.LogInformation("Successfully saved {Count} tracks", request.Ids.Count);
                return Ok(new { message = "Tracks saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tracks");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE /api/tracks/remove - Remove saved tracks
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveSavedTracks(
            [FromHeader] string Authorization,
            [FromBody] SaveTracksRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("RemoveSavedTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (request?.Ids == null || request.Ids.Count == 0)
                {
                    _logger.LogWarning("RemoveSavedTracks called with empty track IDs");
                    return BadRequest("Track IDs are required.");
                }

                if (request.Ids.Count > 50)
                {
                    _logger.LogWarning("RemoveSavedTracks called with too many IDs: {Count}", request.Ids.Count);
                    return BadRequest("Maximum 50 track IDs allowed.");
                }

                var url = "https://api.spotify.com/v1/me/tracks";
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
                requestMessage.Headers.Add("Authorization", Authorization);

                var requestBody = new { ids = request.Ids };
                var json = JsonSerializer.Serialize(requestBody);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Removing {Count} saved tracks", request.Ids.Count);

                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to remove saved tracks: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                _logger.LogInformation("Successfully removed {Count} saved tracks", request.Ids.Count);
                return Ok(new { message = "Tracks removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved tracks");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET /api/tracks/check-saved - Check if tracks are saved
        [HttpGet("check-saved")]
        public async Task<IActionResult> CheckSavedTracks(
            [FromHeader] string Authorization,
            [FromQuery] string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization))
                {
                    _logger.LogWarning("CheckSavedTracks called without Authorization header");
                    return Unauthorized("Bearer token is required.");
                }

                if (string.IsNullOrEmpty(ids))
                {
                    _logger.LogWarning("CheckSavedTracks called with empty IDs");
                    return BadRequest("Track IDs are required.");
                }

                // Validate track IDs count (max 50)
                var trackIds = ids.Split(',');
                if (trackIds.Length > 50)
                {
                    _logger.LogWarning("CheckSavedTracks called with too many IDs: {Count}", trackIds.Length);
                    return BadRequest("Maximum 50 track IDs allowed.");
                }

                var url = $"https://api.spotify.com/v1/me/tracks/contains?ids={ids}";

                _logger.LogInformation("Checking if {Count} tracks are saved", trackIds.Length);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to check saved tracks: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully checked saved status for {Count} tracks", trackIds.Length);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking saved tracks");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class SaveTracksRequest
    {
        public List<string> Ids { get; set; } = new();
    }
}

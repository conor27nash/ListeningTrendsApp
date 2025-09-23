using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TopItemsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopArtistsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TopArtistsController> _logger;

        public TopArtistsController(HttpClient httpClient, ILogger<TopArtistsController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("top-artists/{timeRange}")]
        public async Task<IActionResult> GetTopArtists(
            [FromRoute] [RegularExpression("^(short_term|medium_term|long_term)$", ErrorMessage = "Time range must be short_term, medium_term, or long_term")] 
            string timeRange, 
            [FromHeader] [Required] string Authorization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(Authorization) || !Authorization.StartsWith("Bearer "))
            {
                _logger.LogWarning("Invalid or missing Authorization header");
                return Unauthorized("Bearer token is required");
            }

            try
            {
                _logger.LogInformation("Fetching top artists for time range: {TimeRange}", timeRange);

                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.spotify.com/v1/me/top/artists?time_range={timeRange}&limit=50");
                request.Headers.Add("Authorization", Authorization);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Spotify API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => Unauthorized("Invalid or expired Spotify token"),
                        System.Net.HttpStatusCode.Forbidden => Forbid("Insufficient permissions"),
                        System.Net.HttpStatusCode.TooManyRequests => StatusCode(429, "Rate limit exceeded"),
                        _ => StatusCode(502, "External service error")
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var topArtists = JsonSerializer.Deserialize<object>(json);

                _logger.LogInformation("Successfully retrieved top artists");
                return Ok(topArtists);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while calling Spotify API");
                return StatusCode(502, "Unable to connect to Spotify API");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while calling Spotify API");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetTopArtists");
                return StatusCode(500, "An unexpected error occurred");
            }
        }
    }
}

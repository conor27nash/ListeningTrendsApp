using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecentlyPlayedService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecentlyPlayedController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public RecentlyPlayedController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("recent-tracks")]
        public async Task<IActionResult> GetRecentlyPlayed(
            [FromHeader] string Authorization,
            [FromQuery] int limit = 50,
            [FromQuery] long? after = null,
            [FromQuery] long? before = null)
        {
            if (string.IsNullOrEmpty(Authorization))
            {
                return Unauthorized("Bearer token is required.");
            }

            var queryParams = new List<string> { $"limit={Math.Min(Math.Max(limit, 1), 50)}" };
            if (after.HasValue) queryParams.Add($"after={after.Value}");
            if (before.HasValue) queryParams.Add($"before={before.Value}");
            
            var queryString = string.Join("&", queryParams);
            var url = $"https://api.spotify.com/v1/me/player/recently-played?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", Authorization);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var json = await response.Content.ReadAsStringAsync();
            var recentlyPlayed = JsonSerializer.Deserialize<object>(json);

            return Ok(recentlyPlayed);
        }
    }
}

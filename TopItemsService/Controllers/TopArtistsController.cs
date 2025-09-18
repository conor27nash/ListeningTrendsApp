using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopItemsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopArtistsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public TopArtistsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("top-artists/{timeRange}")]
        public async Task<IActionResult> GetTopArtists(string timeRange, [FromHeader] string Authorization)
        {
            if (string.IsNullOrEmpty(Authorization))
            {
                return Unauthorized("Bearer token is required.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/me/top/artists?time_range={timeRange}&limit=50");
            request.Headers.Add("Authorization", Authorization);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }

            var json = await response.Content.ReadAsStringAsync();
            var topArtists = JsonSerializer.Deserialize<object>(json);

            return Ok(topArtists);
        }
    }
}

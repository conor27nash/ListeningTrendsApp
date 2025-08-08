using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/topartists")] 
    public class TopArtistsProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TopArtistsProxyController> _logger;

        public TopArtistsProxyController(IHttpClientFactory httpClientFactory, ILogger<TopArtistsProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("{timeRange}")]
        public async Task<IActionResult> Get(string timeRange)
        {
            _logger.LogDebug("TopArtistsProxyController.Get called with timeRange={timeRange}", timeRange);
            
            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found; returning Unauthorized");
                return Unauthorized("No access token available");
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);

            var client = _httpClientFactory.CreateClient("TopItemsService");
            _logger.LogInformation("Calling TopItemsService GET {Url}", client.BaseAddress + $"/api/topartists/top-artists/{timeRange}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo);

            var response = await client.GetAsync($"/api/topartists/top-artists/{timeRange}");
            _logger.LogDebug("TopItemsService responded with status {StatusCode}", response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response content length: {Length}", content?.Length ?? 0);
            return StatusCode((int)response.StatusCode, content);
        }
    }
}

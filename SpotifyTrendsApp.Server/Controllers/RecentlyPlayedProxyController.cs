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
    [Route("api/recentlyplayed")] 
    public class RecentlyPlayedProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RecentlyPlayedProxyController> _logger;

        public RecentlyPlayedProxyController(IHttpClientFactory httpClientFactory, ILogger<RecentlyPlayedProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("recent-tracks")]
        public async Task<IActionResult> GetRecentTracks(
            [FromQuery] int limit = 20,
            [FromQuery] long? after = null,
            [FromQuery] long? before = null)
        {
            _logger.LogDebug("RecentlyPlayedProxyController.GetRecentTracks called with limit={limit}, after={after}, before={before}", limit, after, before);
            
            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found; returning Unauthorized");
                return Unauthorized("No access token available");
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);

            var client = _httpClientFactory.CreateClient("RecentlyPlayedService");
            
            var queryParams = new List<string> { $"limit={limit}" };
            if (after.HasValue) queryParams.Add($"after={after.Value}");
            if (before.HasValue) queryParams.Add($"before={before.Value}");
            var queryString = string.Join("&", queryParams);
            
            var url = $"/api/recentlyplayed/recent-tracks?{queryString}";
            _logger.LogInformation("Calling RecentlyPlayedService GET {Url}", client.BaseAddress + url);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo);

            var response = await client.GetAsync(url);
            _logger.LogDebug("RecentlyPlayedService responded with status {StatusCode}", response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response content length: {Length}", content?.Length ?? 0);
            return StatusCode((int)response.StatusCode, content);
        }
    }
}

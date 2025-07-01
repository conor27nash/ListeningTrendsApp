using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using SpotifyTrendsApp.Server.Services;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/toptracks")] 
    public class TopTracksProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TopTracksProxyController> _logger;

        public TopTracksProxyController(IHttpClientFactory httpClientFactory, ILogger<TopTracksProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("{timeRange}")]
        public async Task<IActionResult> Get(
            string timeRange)
        {
            _logger.LogDebug("TopTracksProxyController.Get called with timeRange={timeRange}", timeRange);

            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found; returning Unauthorized");
                return Unauthorized("No access token available");
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);

            var client = _httpClientFactory.CreateClient("TopItemsService");
            _logger.LogInformation("Calling TopItemsService GET {Url}", client.BaseAddress + $"/api/toptracks/top-tracks/{timeRange}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo);

            var response = await client.GetAsync($"/api/toptracks/top-tracks/{timeRange}");
            _logger.LogDebug("TopItemsService responded with status {StatusCode}", response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response content length: {Length}", content?.Length ?? 0);
            return StatusCode((int)response.StatusCode, content);
        }
    }
}

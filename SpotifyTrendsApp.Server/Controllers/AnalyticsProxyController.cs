using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http.Json;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AnalyticsProxyController> _logger;

        public AnalyticsProxyController(IHttpClientFactory httpClientFactory, ILogger<AnalyticsProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint - no authorization required
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            _logger.LogDebug("AnalyticsProxyController.GetHealth called");

            var client = _httpClientFactory.CreateClient("AnalyticsService");
            _logger.LogInformation("Calling AnalyticsService GET {Url}", client.BaseAddress + "/api/analytics/health");

            var response = await client.GetAsync("/api/analytics/health");
            _logger.LogDebug("AnalyticsService health check responded with status {StatusCode}", response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        /// <summary>
        /// Get analytics data - requires authorization
        /// </summary>
        [HttpGet("analytics")]
        [Authorize]
        public async Task<IActionResult> GetAnalytics(string timeRange = "medium_term")
        {
            _logger.LogDebug("AnalyticsProxyController.GetAnalytics called");

            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found; returning Unauthorized");
                return Unauthorized("No access token available");
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);

            var client = _httpClientFactory.CreateClient("AnalyticsService");
            _logger.LogInformation("Calling AnalyticsService GET {Url}", client.BaseAddress + "/api/analytics/analytics");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo);

            var response = await client.GetAsync($"/api/analytics/analytics?timerange={timeRange}");
            _logger.LogDebug("AnalyticsService responded with status {StatusCode}", response.StatusCode);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            var analyticsDto = await response.Content.ReadFromJsonAsync<AnalyticsDto>(jsonOptions);
            return Ok(analyticsDto);
        }

        /// <summary>
        /// Refresh analytics data - requires authorization
        /// </summary>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshAnalytics()
        {
            _logger.LogDebug("AnalyticsProxyController.RefreshAnalytics called");

            var tokenInfo = User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(tokenInfo))
            {
                _logger.LogWarning("No access_token claim found; returning Unauthorized");
                return Unauthorized("No access token available");
            }
            _logger.LogDebug("Using access_token prefix={TokenPrefix}...", tokenInfo.Length > 8 ? tokenInfo.Substring(0, 8) : tokenInfo);

            var client = _httpClientFactory.CreateClient("AnalyticsService");
            _logger.LogInformation("Calling AnalyticsService POST {Url}", client.BaseAddress + "/api/analytics/refresh");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo);

            var response = await client.PostAsync("/api/analytics/refresh", null);
            _logger.LogDebug("AnalyticsService responded with status {StatusCode}", response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response content length: {Length}", content?.Length ?? 0);
            return StatusCode((int)response.StatusCode, content);
        }
    }
}

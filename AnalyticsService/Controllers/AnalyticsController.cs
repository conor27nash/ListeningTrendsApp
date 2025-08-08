using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using SpotifyTrendsApp.Common.Models;
using System.Text.Json;
using SpotifyTrends.AnalyticsService.Services;

namespace AnalyticsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly SpotifyTrends.AnalyticsService.Services.AnalyticsService _analyticsService;

    public AnalyticsController(IHttpClientFactory httpClientFactory, ILogger<AnalyticsController> logger, SpotifyTrends.AnalyticsService.Services.AnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "AnalyticsService" });
    }

    /// <summary>
    /// Get analytics data for the current user
    /// </summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics([FromHeader] string Authorization, string timeRange = "medium_term")
    {
        try
        {
            if (string.IsNullOrEmpty(Authorization))
            {
                return Unauthorized("Authorization header is required");
            }

            _logger.LogInformation("Getting analytics data for timeRange: {TimeRange}", timeRange);

            // Call AnalyticsService to generate analytics
            var analyticsData = await _analyticsService.GenerateAnalyticsAsync(timeRange, Authorization);

            return Ok(analyticsData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics");
            return StatusCode(500, new { message = "Error generating analytics", error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh analytics data
    /// </summary>
    [HttpPost("refresh")]
    public IActionResult RefreshAnalytics([FromHeader] string? Authorization)
    {
        try
        {
            if (string.IsNullOrEmpty(Authorization))
            {
                return Unauthorized("Authorization header is required");
            }

            _logger.LogInformation("Refreshing analytics data");

            // For now, just return the same as GetAnalytics but with a refresh message
            return Ok(new
            {
                message = "Analytics data refreshed successfully",
                timestamp = DateTime.UtcNow,
                status = "refreshed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing analytics");
            return StatusCode(500, "Internal server error");
        }
    }
}

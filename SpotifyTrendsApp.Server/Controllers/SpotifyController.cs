using Microsoft.AspNetCore.Mvc;
using SpotifyTrendsApp.Server.Services;
using System.Threading.Tasks;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyController : ControllerBase
    {
        private readonly ISpotifyService _spotifyService;

        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        [HttpGet("top-tracks")]
        public async Task<IActionResult> GetTopTracks()
        {
            var topTracks = await _spotifyService.GetTopTracksAsync();
            return Ok(topTracks);
        }

        [HttpGet("top-artists")]
        public async Task<IActionResult> GetTopArtists()
        {
            var topArtists = await _spotifyService.GetTopArtistsAsync();
            return Ok(topArtists);
        }
    }
}

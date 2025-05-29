using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SpotifyTrendsApp.Server.Services
{
    public class SpotifyService : ISpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SpotifyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<IEnumerable<object>> GetTopTracksAsync()
        {
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me/top/tracks");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<object>>(json);
        }

        public async Task<IEnumerable<object>> GetTopArtistsAsync()
        {
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me/top/artists");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<object>>(json);
        }
    }
}

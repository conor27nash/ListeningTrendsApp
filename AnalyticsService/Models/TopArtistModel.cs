using SpotifyTrendsApp.Common.Models;

namespace AnalyticsService.Models
{
    public class TopArtist
    {
        public string ArtistName { get; set; } = string.Empty;
        public string SpotifyLink { get; set; } = string.Empty;
        public int Count { get; set; }
        public Track[] Tracks { get; set; } = Array.Empty<Track>();
    }
}
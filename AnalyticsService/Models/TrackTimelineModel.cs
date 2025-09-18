namespace AnalyticsService.Models
{
    public class TrackTimeline
    {
        public string TrackName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string SpotifyLink { get; set; } = string.Empty;
    }
}
namespace AnalyticsService.Models
{
    public class ArtistLeaderboard
    {
        public string ArtistName { get; set; } = string.Empty;
        public string SpotifyLink { get; set; } = string.Empty;
        public int Rank { get; set; }
        public int FollowerCount { get; set; }
        public int Popularity { get; set; }
    }

}
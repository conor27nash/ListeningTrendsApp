namespace AnalyticsService.Models
{
    public class AnalyticsData
    {
        public List<AlbumMosaic> AlbumMosaicData { get; set; } = new List<AlbumMosaic>();
        public TopArtist TopArtistData { get; set; } = new TopArtist();
        public List<ArtistLeaderboard> ArtistLeaderboardData { get; set; } = new List<ArtistLeaderboard>();
        public List<TrackTimeline> TrackTimelineData { get; set; } = new List<TrackTimeline>();
        public List<GenreBubble> GenreBubbleData { get; set; } = new List<GenreBubble>();
    }

}
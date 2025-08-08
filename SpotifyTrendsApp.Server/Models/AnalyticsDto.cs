using System.Collections.Generic;

namespace SpotifyTrendsApp.Server.Models
{
    public class AnalyticsDto
    {
        public IEnumerable<AlbumMosaicDto> AlbumMosaicData { get; set; }
        public TopArtistDto TopArtistData { get; set; }
        public IEnumerable<ArtistLeaderboardDto> ArtistLeaderboardData { get; set; }
        public IEnumerable<TrackTimelineDto> TrackTimelineData { get; set; }
        public IEnumerable<GenreBubbleDto> GenreBubbleData { get; set; }
    }

    public class AlbumMosaicDto
    {
        public string AlbumName { get; set; }
        public string ArtistName { get; set; }
        public string SpotifyLink { get; set; }
        public int Count { get; set; }
    }

    public class TrackTimelineDto
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string SpotifyLink { get; set; }
    }

    public class ArtistLeaderboardDto
    {
        public string ArtistName { get; set; }
        public string SpotifyLink { get; set; }
        public int Rank { get; set; }
        public int FollowerCount { get; set; }
        public int Popularity { get; set; }
    }
    public class TopArtistDto
    {
        public string ArtistName { get; set; }
        public string SpotifyLink { get; set; }
        public int Count { get; set; }
        public IEnumerable<TrackDto> Tracks { get; set; }
    }
    public class TrackDto
    {
        public string TrackName { get; set; }
        public string SpotifyLink { get; set; }
    }
    public class GenreBubbleDto
    {
        public string Genre { get; set; }
        public int Count { get; set; }
    }
}
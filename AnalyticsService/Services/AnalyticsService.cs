using System;
using SpotifyTrendsApp.Common.Models;
using AnalyticsService.Models;

namespace SpotifyTrends.AnalyticsService.Services
{
    public class AnalyticsService
    {
        private readonly TopItemsService _topItemsService;
        public AnalyticsService(TopItemsService topItemsService)
        {
            _topItemsService = topItemsService;

        }

        // Add your service methods below
        public async Task<AnalyticsData> GenerateAnalyticsAsync(string timeRange, string authorization)
        {
            // Call TopItemsService to get top tracks and artists
            var topTracks = await _topItemsService.GetTopTracks(timeRange, authorization);
            var topArtists = await _topItemsService.GetTopArtists(timeRange, authorization);


            return new AnalyticsData
            {
                AlbumMosaicData = await GenerateAlbumMosaicAsync(topTracks),
                TopArtistData = await GenerateTopArtist(topTracks),
                ArtistLeaderboardData = await GenerateArtistLeaderboard(topArtists),
                TrackTimelineData = await GenerateTrackTimeline(topTracks),
                GenreBubbleData = await GenerateGenreBubbleChart(topArtists)
            };

        }

        public Task<List<AlbumMosaic>> GenerateAlbumMosaicAsync(List<Track> topTracks)
        {
            var albumMosaic = topTracks
                .GroupBy(track => track.Album.Name)
                .Select(g => new AlbumMosaic
                {
                    AlbumName = g.Key,
                    ArtistName = string.Join(", ", g.First().Artists.Select(artist => artist.Name)),
                    SpotifyLink = g.First().Album.uri ?? string.Empty,
                    Count = g.Count()
                }).ToList();

            return Task.FromResult(albumMosaic);
        }

        public Task<TopArtist> GenerateTopArtist(List<Track> topTracksop)
        {
        // Find the artist that appears most frequently in the tracks
        if (topTracksop == null || topTracksop.Count == 0)
        {
            return Task.FromResult(new TopArtist
            {
                ArtistName = "No Artist Found",
                SpotifyLink = "",
                Count = 0,
                Tracks = Array.Empty<Track>()
            });
        }

        var artistCounts = new Dictionary<string, (int count, string uri, List<Track> tracks)>();

        foreach (var track in topTracksop)
        {
            foreach (var artist in track.Artists)
            {
                if (artist == null || string.IsNullOrEmpty(artist.Name)) continue;
                if (!artistCounts.ContainsKey(artist.Name))
                {
                    artistCounts[artist.Name] = (1, artist.uri ?? string.Empty, new List<Track> { track });
                }
                else
                {
                    var entry = artistCounts[artist.Name];
                    entry.count++;
                    entry.tracks.Add(track);
                    artistCounts[artist.Name] = entry;
                }
            }
        }

        var topArtistEntry = artistCounts.OrderByDescending(a => a.Value.count).FirstOrDefault();

        var topArtist = new TopArtist
        {
            ArtistName = topArtistEntry.Key ?? "No Artist Found",
            SpotifyLink = topArtistEntry.Value.uri,
            Count = topArtistEntry.Value.count,
            Tracks = topArtistEntry.Value.tracks.ToArray()
        };

        return Task.FromResult(topArtist);
        }

        public Task<List<ArtistLeaderboard>> GenerateArtistLeaderboard(List<Artist> topArtists)
        {
            var artistLeaderboard = topArtists
                .Select((artist, index) => new ArtistLeaderboard
                {
                    ArtistName = artist.Name,
                    SpotifyLink = artist.uri ?? string.Empty,
                    Rank = 100 - (index * 2),
                    FollowerCount = artist.Followers,
                    Popularity = artist.popularity
                })
                .OrderByDescending(leaderboard => leaderboard.Rank)
                .ToList();

            return Task.FromResult(artistLeaderboard);
        }

        public Task<List<TrackTimeline>> GenerateTrackTimeline(List<Track> topTracks)
        {
            var trackTimeline = topTracks
                .Where(track => {
                    var releaseDate = track.Album?.release_date;
                    if (string.IsNullOrWhiteSpace(releaseDate)) return false;
                    if (releaseDate == "0" || releaseDate == "0000") return false;
                    DateTime parsedDate;
                    if (!DateTime.TryParse(releaseDate, out parsedDate)) return false;
                    return parsedDate.Year > 1900; // Only allow plausible years
                })
                .Select(track => new TrackTimeline
                {
                    TrackName = track.Name,
                    ArtistName = string.Join(", ", track.Artists.Select(artist => artist.Name)),
                    ReleaseDate = DateTime.TryParse(track.Album.release_date, out var date) ? date : DateTime.MinValue,
                    SpotifyLink = track.uri ?? string.Empty
                })
                .OrderBy(timeline => timeline.ReleaseDate)
                .ToList();

            return Task.FromResult(trackTimeline);
        }

        public Task<List<GenreBubble>> GenerateGenreBubbleChart(List<Artist> topArtists)
        {
            var genreCounts = new Dictionary<string, int>();

            foreach (var artist in topArtists)
            {
                if (artist.Genres != null)
                {
                    foreach (var genre in artist.Genres)
                    {
                        if (genreCounts.ContainsKey(genre))
                        {
                            genreCounts[genre]++;
                        }
                        else
                        {
                            genreCounts[genre] = 1;
                        }
                    }
                }
            }

            var genreBubbles = genreCounts.Select(g => new GenreBubble
            {
                Genre = g.Key,
                Count = g.Value
            }).OrderByDescending(b => b.Count).ToList();

            return Task.FromResult(genreBubbles);
        }


        public Task<MusicDNA> GenerateMusicDNA(List<Track> topTracks, List<Artist> topArtists)
        {
            // TODO: Implement MusicDNA generation logic
            var musicDNA = new MusicDNA
            {
                TopGenres = topArtists.SelectMany(a => a.Genres ??  [""]).Distinct().Take(5).ToList(),
                AveragePopularity = topTracks.Any() ? topTracks.Average(t => t.popularity) : 0,
                TotalTracks = topTracks.Count,
                TotalArtists = topArtists.Count
            };
            return Task.FromResult(musicDNA);
        }

        public Task<List<FollowVsTop>> GenerateFollowedVsTopArtist(List<Track> topTracks, List<Artist> followedArtists)
        {
            // TODO: Implement FollowedVsTopArtist logic
            var followVsTop = new List<FollowVsTop>();
            
            return Task.FromResult(followVsTop);
        } 
    }
}
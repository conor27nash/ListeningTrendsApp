using System;
using SpotifyTrendsApp.Common.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SpotifyTrends.AnalyticsService.Services
{
    public class TopItemsService
    {
        private readonly HttpClient _topItemsClient;
        private readonly ILogger<TopItemsService> _logger;
        
        public TopItemsService(IHttpClientFactory httpClientFactory, ILogger<TopItemsService> logger)
        {
            _topItemsClient = httpClientFactory.CreateClient("TopItemsService");
            _logger = logger;
        }

        public async Task<List<Track>> GetTopTracks(string time_range, string Authorization)
        {
            try
            {
                _topItemsClient.DefaultRequestHeaders.Clear();
                _topItemsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authorization.Replace("Bearer ", ""));

                var topTracksResponse = await _topItemsClient.GetAsync($"api/toptracks/top-tracks/{time_range}");

                if (!topTracksResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get top tracks: {StatusCode}", topTracksResponse.StatusCode);
                    return new List<Track>();
                }

                var topTracksData = await topTracksResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw response from TopItems service: {Content}", topTracksData.Length > 500 ? topTracksData.Substring(0, 500) + "..." : topTracksData);

                // Parse JSON manually to avoid serialization conflicts
                var tracks = ParseTracksFromJson(topTracksData);
                return tracks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top tracks from TopItems service");
                return new List<Track>();
            }
        }

        public async Task<List<Artist>> GetTopArtists(string time_range, string Authorization)
        {
            try
            {
                _topItemsClient.DefaultRequestHeaders.Clear();
                _topItemsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Authorization.Replace("Bearer ", ""));

                var topArtistsResponse = await _topItemsClient.GetAsync($"api/topartists/top-artists/{time_range}");

                if (!topArtistsResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get top artists: {StatusCode}", topArtistsResponse.StatusCode);
                    return new List<Artist>();
                }

                var topArtistsData = await topArtistsResponse.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw response from TopItems service: {Content}", topArtistsData.Length > 500 ? topArtistsData.Substring(0, 500) + "..." : topArtistsData);

                // Parse JSON manually to avoid serialization conflicts
                var artists = ParseArtistsFromJson(topArtistsData);
                return artists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top artists from TopItems service");
                return new List<Artist>();
            }
        }

        private List<Track> ParseTracksFromJson(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                var tracks = new List<Track>();

                // Check if response has "items" array (Spotify API format)
                JsonElement itemsElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out itemsElement))
                {
                    // Response is { "items": [...] }
                    foreach (var item in itemsElement.EnumerateArray())
                    {
                        tracks.Add(ParseTrackFromElement(item));
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    // Response is direct array [...]
                    foreach (var item in root.EnumerateArray())
                    {
                        tracks.Add(ParseTrackFromElement(item));
                    }
                }

                return tracks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing tracks from JSON");
                return new List<Track>();
            }
        }

        private List<Artist> ParseArtistsFromJson(string jsonContent)
        {
            try
            {
                using var document = JsonDocument.Parse(jsonContent);
                var root = document.RootElement;
                var artists = new List<Artist>();

                // Check if response has "items" array (Spotify API format)
                JsonElement itemsElement;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out itemsElement))
                {
                    // Response is { "items": [...] }
                    foreach (var item in itemsElement.EnumerateArray())
                    {
                        artists.Add(ParseArtistFromElement(item));
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    // Response is direct array [...]
                    foreach (var item in root.EnumerateArray())
                    {
                        artists.Add(ParseArtistFromElement(item));
                    }
                }

                return artists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing artists from JSON");
                return new List<Artist>();
            }
        }

        private Track ParseTrackFromElement(JsonElement element)
        {
            return new Track
            {
                Id = GetStringProperty(element, "id"),
                Name = GetStringProperty(element, "name"),
                uri = GetStringProperty(element, "uri"),
                href = GetStringProperty(element, "href"),
                popularity = GetIntProperty(element, "popularity"),
                duration_ms = GetIntProperty(element, "duration_ms"),
                _explicit = GetBoolProperty(element, "explicit"),
                Album = ParseAlbumFromElement(element, "album"),
                Artists = ParseArtistsArrayFromElement(element, "artists")
            };
        }

        private Artist ParseArtistFromElement(JsonElement element)
        {
            return new Artist
            {
                Id = GetStringProperty(element, "id"),
                Name = GetStringProperty(element, "name"),
                uri = GetStringProperty(element, "uri"),
                href = GetStringProperty(element, "href"),
                popularity = GetIntProperty(element, "popularity"),
                Genres = ParseStringArrayFromElement(element, "genres").ToArray(),
                Followers = ParseFollowersFromElement(element, "followers").total,
                images = ParseImagesArrayFromElement(element, "images")
            };
        }

        private Album ParseAlbumFromElement(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var albumElement))
            {
                return new Album
                {
                    Id = GetStringProperty(albumElement, "id"),
                    Name = GetStringProperty(albumElement, "name"),
                    uri = GetStringProperty(albumElement, "uri"),
                    href = GetStringProperty(albumElement, "href"),
                    release_date = GetStringProperty(albumElement, "release_date"),
                    album_type = GetStringProperty(albumElement, "album_type"),
                    total_tracks = GetIntProperty(albumElement, "total_tracks"),
                    Artists = ParseArtistsArrayFromElement(albumElement, "artists").Select(a => a.Name).ToArray(),
                    images = ParseImagesArrayFromElement(albumElement, "images")
                };
            }
            return new Album();
        }

        private List<Artist> ParseArtistsArrayFromElement(JsonElement element, string propertyName)
        {
            var artists = new List<Artist>();
            if (element.TryGetProperty(propertyName, out var artistsElement) && artistsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var artistElement in artistsElement.EnumerateArray())
                {
                    artists.Add(new Artist
                    {
                        Id = GetStringProperty(artistElement, "id"),
                        Name = GetStringProperty(artistElement, "name"),
                        uri = GetStringProperty(artistElement, "uri"),
                        href = GetStringProperty(artistElement, "href")
                    });
                }
            }
            return artists;
        }

        private List<string> ParseStringArrayFromElement(JsonElement element, string propertyName)
        {
            var items = new List<string>();
            if (element.TryGetProperty(propertyName, out var arrayElement) && arrayElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arrayElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        items.Add(item.GetString() ?? "");
                    }
                }
            }
            return items;
        }

        private List<Image> ParseImagesArrayFromElement(JsonElement element, string propertyName)
        {
            var images = new List<Image>();
            if (element.TryGetProperty(propertyName, out var imagesElement) && imagesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var imageElement in imagesElement.EnumerateArray())
                {
                    images.Add(new Image
                    {
                        url = GetStringProperty(imageElement, "url"),
                        height = GetIntProperty(imageElement, "height"),
                        width = GetIntProperty(imageElement, "width")
                    });
                }
            }
            return images;
        }

        private Followers ParseFollowersFromElement(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var followersElement))
            {
                return new Followers
                {
                    href = GetStringProperty(followersElement, "href"),
                    total = GetIntProperty(followersElement, "total")
                };
            }
            return new Followers();
        }

        private string GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) ? (property.ValueKind == JsonValueKind.String ? property.GetString() ?? "" : "") : "";
        }

        private int GetIntProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind == JsonValueKind.Number ? property.GetInt32() : 0;
            }
            return 0;
        }

        private bool GetBoolProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind == JsonValueKind.True;
            }
            return false;
        }
    }
}
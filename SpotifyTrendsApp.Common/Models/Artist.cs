using System.Collections.Generic;

namespace SpotifyTrendsApp.Common.Models;

public class Artist
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string[] Genres { get; set; }
    public int Followers { get; set; }
    public string ImageUrl { get; set; }
    public IEnumerable<string> available_markets { get; set; }
    public string href { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
    public List<Image> images { get; set; }
    public int popularity { get; set; }
}

public class ArtistResponse
{
    public string href { get; set; }
    public int limit { get; set; }
    public List<Artist> items { get; set; }
    public int offset { get; set; }
    public int total { get; set; }
}

public class Followers
{
    public string? href { get; set; }
    public int total { get; set; }
}


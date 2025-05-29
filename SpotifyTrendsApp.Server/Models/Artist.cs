using System;

namespace SpotifyTrendsWebApp.Server.Models;

public class ArtistResponse
{
    public string href { get; set; }
    public int limit { get; set; }
    public List<Artist> items { get; set; }
    public int offset { get; set; }
    public int total { get; set; }
}
public class Artist
{
    public Followers followers { get; set; }
    public IEnumerable<string> genres { get; set; }
    public string href { get; set; }
    public string id { get; set; }
    public IEnumerable<Image> images { get; set; }
    public string name { get; set; }
    public string type { get; set; } = "artist";
    public int popularity { get; set; }
    public string uri { get; set; }
}

public class Followers
{
    public string? href { get; set; }
    public int total { get; set; }
}
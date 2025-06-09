using System.Collections.Generic;

namespace SpotifyTrendsApp.Common.Models;

public class Track
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Album Album { get; set; }
    public IEnumerable<Artist> Artists { get; set; }
    public IEnumerable<string> available_markets { get; set; }
    public int disc_number { get; set; }
    public int duration_ms { get; set; }
    public bool _explicit { get; set; }
    public string href { get; set; }
    public bool is_playable { get; set; }
    public int popularity { get; set; }
    public object preview_url { get; set; }
    public int track_number { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
    public bool is_local { get; set; }
}

public class TrackResponse
{
    public string href { get; set; }
    public int limit { get; set; }
    public List<Track> items { get; set; }
    public int offset { get; set; }
    public int total { get; set; }
}


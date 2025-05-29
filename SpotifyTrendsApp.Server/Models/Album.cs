using System;

namespace SpotifyTrendsWebApp.Server.Models;

public class AlbumResponse
{
    public string href { get; set; }
    public int limit { get; set; }
    public List<Album> items { get; set; }
    public int offset { get; set; }
    public int total { get; set; }
}
public class Album
{
    public string album_type { get; set; }
    public int total_tracks { get; set; }
    public List<string> available_markets { get; set; }
    public string href { get; set; }
    public string id { get; set; }
    public List<Image> images { get; set; }
    public string name { get; set; }
    public string release_date { get; set; }
    public string release_date_precision { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
    public List<Artist> artists { get; set; }
    public IEnumerable<TrackResponse> tracks { get; set; }
    public Copyright copyright { get; set; }
    public IEnumerable<string> genres { get; set; }
    public string label { get; set; }
    public int popularity { get; set; }
    public bool is_playable { get; set; }
}

public class Copyright
{
    public string text { get; set; }
    public string type { get; set; }
}

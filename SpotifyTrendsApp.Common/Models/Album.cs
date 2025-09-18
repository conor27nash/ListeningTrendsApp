using System.Collections.Generic;
namespace SpotifyTrendsApp.Common.Models;

public class Album
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string[] Artists { get; set; }
    public string ImageUrl { get; set; }
    public IEnumerable<string> available_markets { get; set; }
    public string href { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
    public string album_type { get; set; }
    public int total_tracks { get; set; }
    public List<string> markets { get; set; }
    public List<Image> images { get; set; }
    public string release_date { get; set; }
    public string release_date_precision { get; set; }
    public IEnumerable<TrackResponse> tracks { get; set; }
    public Copyright copyright { get; set; }
    public IEnumerable<string> genres { get; set; }
    public string label { get; set; }
    public int popularity { get; set; }
    public bool is_playable { get; set; }
}

public class AlbumResponse
{
    public string href { get; set; }
    public int limit { get; set; }
    public List<Album> items { get; set; }
    public int offset { get; set; }
    public int total { get; set; }
}

public class Copyright
{
    public string text { get; set; }
    public string type { get; set; }
}


using System;

namespace SpotifyTrendsWebApp.Server.Models;

public class User
{
    public string display_name { get; set; }
    public Followers followers { get; set; }
    public string href { get; set; }
    public string id { get; set; }
    public List<object> images { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
}
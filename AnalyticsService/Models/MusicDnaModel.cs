namespace AnalyticsService.Models;
public class MusicDNA
{
    public List<string> TopGenres { get; set; } = new();
    public double AveragePopularity { get; set; }
    public int TotalTracks { get; set; }
    public int TotalArtists { get; set; }
}
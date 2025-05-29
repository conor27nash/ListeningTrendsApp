using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotifyTrendsApp.Server.Services
{
    public interface ISpotifyService
    {
        Task<IEnumerable<object>> GetTopTracksAsync();
        Task<IEnumerable<object>> GetTopArtistsAsync();
    }
}

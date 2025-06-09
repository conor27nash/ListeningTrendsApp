using Microsoft.AspNetCore.Authentication.OAuth;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Server.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private TokenInfo _currentToken;
    public TokenInfo CurrentToken => _currentToken;

    public TokenService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenInfo> GetAccessTokenAsync(string code)
    {   
        try
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var redirectUri = _configuration["Spotify:RedirectUri"];

            _logger.LogInformation("Requesting access token for authorization code");

            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var requestContent = new FormUrlEncodedContent(requestBody);

            _logger.LogInformation("Sending request to Spotify API: {RequestBody}", requestBody);

            var response = await _httpClient.PostAsync("api/token", requestContent);

            _logger.LogInformation("Received response from Spotify API: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API returned error: {ErrorContent}", errorContent);
            }

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>();
            
            if (responseBody == null)
            {
                throw new InvalidOperationException("Failed to deserialize OAuth token response");
            }

            _currentToken = new TokenInfo
            {
                AccessToken = responseBody.AccessToken,
                RefreshToken = responseBody.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(double.Parse(responseBody.ExpiresIn))
            };

            return _currentToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to obtain access token from Spotify");
            throw;
        }
    }

   public async Task<TokenInfo> RefreshAccessTokenAsync(string refreshToken)
{
    try
    {
        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];

        _logger.LogInformation("Refreshing access token");

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var requestContent = new FormUrlEncodedContent(requestBody);

        var response = await _httpClient.PostAsync("api/token", requestContent);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>();
        
        if (responseBody == null)
        {
            throw new InvalidOperationException("Failed to deserialize OAuth token response");
        }

        _currentToken = new TokenInfo
        {
            AccessToken = responseBody.AccessToken,
            // Keep the existing refresh token if a new one isn't provided
            RefreshToken = responseBody.RefreshToken ?? _currentToken?.RefreshToken ?? refreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(double.Parse(responseBody.ExpiresIn))
        };

        _logger.LogInformation("Successfully refreshed access token");
        return _currentToken;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to refresh access token from Spotify");
        throw;
    }
}
}
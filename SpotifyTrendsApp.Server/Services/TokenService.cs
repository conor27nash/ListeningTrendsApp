using System.Net.Http.Headers;
using System.Text;
using SpotifyTrendsApp.Server.Models;

namespace SpotifyTrendsApp.Server.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private TokenInfo _currentToken;

    public TokenInfo CurrentToken
    {
        get => _currentToken;
        set => _currentToken = value;
    }

    public TokenService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _currentToken = null!;
    }

    public async Task<TokenInfo> GetAccessTokenAsync(string code)
    {
        try
        {
            var clientId = _configuration["Spotify:ClientId"] ?? throw new InvalidOperationException("ClientId is not configured");
            var clientSecret = _configuration["Spotify:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret is not configured");
            var redirectUri = _configuration["Spotify:RedirectUri"] ?? throw new InvalidOperationException("RedirectUri is not configured");
            var tokenEndpoint = _configuration["Spotify:TokenEndpoint"] ?? throw new InvalidOperationException("TokenEndpoint is not configured");

            _logger.LogInformation("Requesting access token for authorization code");

            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },

            };
            var authCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authCredentials);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestContent = new FormUrlEncodedContent(requestBody);

            _logger.LogInformation("Sending request to Spotify API: {RequestBody}", requestBody);

            var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);

            _logger.LogInformation("Received response from Spotify API: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify API returned error: {ErrorContent}", errorContent);
            }

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<AuthTokenResponse>() ?? throw new InvalidOperationException("Failed to deserialize OAuth token response");

            var token = new TokenInfo
            {
                AccessToken = responseBody.access_token!,  
                RefreshToken = responseBody.refresh_token ?? string.Empty,
                ExpiresAt = DateTime.UtcNow.AddSeconds(responseBody.expires_in)
            };

            _currentToken = token;
            return token;
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
            var clientId = _configuration["Spotify:ClientId"] ?? throw new InvalidOperationException("ClientId is not configured");
            var clientSecret = _configuration["Spotify:ClientSecret"] ?? throw new InvalidOperationException("ClientSecret is not configured");
            var tokenEndpoint = _configuration["Spotify:TokenEndpoint"] ?? throw new InvalidOperationException("TokenEndpoint is not configured");

            _logger.LogInformation("Refreshing access token");

            var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
        };

            var requestContent = new FormUrlEncodedContent(requestBody);

            var authCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authCredentials);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadFromJsonAsync<AuthTokenResponse>() ?? throw new InvalidOperationException("Failed to deserialize OAuth token response");

            var token = new TokenInfo
            {
                AccessToken = responseBody.access_token!,  
                RefreshToken = responseBody.refresh_token ?? refreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(responseBody.expires_in)
            };

            _currentToken = token;
            _logger.LogInformation("Successfully refreshed access token");
            return token;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to refresh access token from Spotify");
            throw;
        }
    }

    public async Task<string?> GetStoredRefreshTokenAsync()
        => await Task.FromResult(_currentToken?.RefreshToken);

    public async Task<string?> GetStoredAccessTokenAsync()
        => await Task.FromResult(_currentToken?.AccessToken);
}
public class AuthTokenResponse
{
    public string? access_token { get; set; }
    public string? token_type { get; set; }
    public int expires_in { get; set; }
    public string? refresh_token { get; set; }
    public string? scope { get; set; }
}
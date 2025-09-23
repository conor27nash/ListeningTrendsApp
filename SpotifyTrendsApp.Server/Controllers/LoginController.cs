using Microsoft.AspNetCore.Mvc;
using SpotifyTrendsApp.Server.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;

        public LoginController(ILogger<LoginController> logger, IConfiguration config, ITokenService tokenService)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
        }

        [HttpGet("connect")]
        public IActionResult Connect()
        {
            var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") 
                ?? _config["Spotify:ClientId"];
            var redirectUri = Environment.GetEnvironmentVariable("SPOTIFY_REDIRECT_URI") 
                ?? _config["Spotify:RedirectUri"];
            var scopes = _config["Spotify:Scopes"] ?? "user-top-read";
            
            if (string.IsNullOrEmpty(clientId))
                return BadRequest("Spotify ClientId is not configured");
            if (string.IsNullOrEmpty(redirectUri))
                return BadRequest("Spotify RedirectUri is not configured");
                
            var state = Guid.NewGuid().ToString("N");
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = clientId;
            query["response_type"] = "code";
            query["redirect_uri"] = redirectUri;
            query["scope"] = scopes;
            query["state"] = state;
            query["show_dialog"] = "true";
            var url = "https://accounts.spotify.com/authorize?" + query.ToString();
            return Redirect(url);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Missing code");
            var tokenInfo = await _tokenService.GetAccessTokenAsync(code);
            if (tokenInfo == null) return BadRequest("Token exchange failed");
            
            var jwtSection = _config.GetSection("Jwt");
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? jwtSection.GetValue<string>("Key");
            if (string.IsNullOrEmpty(jwtKey))
                return StatusCode(500, "JWT key is not configured");
                
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim("access_token", tokenInfo.AccessToken),
                new Claim("refresh_token", tokenInfo.RefreshToken)
            };
            var jwt = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSection.GetValue<int>("ExpiresInMinutes")),
                signingCredentials: creds);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            var clientUrl = Environment.GetEnvironmentVariable("CLIENT_APP_BASE_URL") 
                ?? _config["ClientApp:BaseUrl"] 
                ?? "http://localhost:5173";
            return Redirect($"{clientUrl}/?token={tokenString}");
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveCode([FromBody] CodeRequest code)
        {
            _logger.LogDebug(code.code);
            if (code == null || string.IsNullOrEmpty(code.code))
            {
                return BadRequest("Invalid token");
            }

            var AccessResponse = await _tokenService.GetAccessTokenAsync(code.code);
            if (AccessResponse == null)
            {
                return BadRequest("Failed to retrieve access code");
            };
        
            _logger.LogInformation($"Received token: {code}");
            
            return Ok(new { message = "Token received successfully" });
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
         {
            var refreshToken = User.FindFirst("refresh_token")?.Value;
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No Spotify refresh token available");
            var updatedToken = await _tokenService.RefreshAccessTokenAsync(refreshToken);
            
            var jwtSection = _config.GetSection("Jwt");
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? jwtSection.GetValue<string>("Key");
            if (string.IsNullOrEmpty(jwtKey))
                return StatusCode(500, "JWT key is not configured");
                
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new Claim("access_token", updatedToken.AccessToken),
                new Claim("refresh_token", updatedToken.RefreshToken)
            };
            var jwt = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSection.GetValue<int>("ExpiresInMinutes")),
                signingCredentials: creds);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            return Ok(new { token = tokenString });
         }

        public class CodeRequest
        {
            public string? code { get; set; }
        }   
    }
}

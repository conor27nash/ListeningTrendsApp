using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyTrendsApp.Server.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.Web;

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

        // Step 1: redirect user to Spotify authorization URL
        [HttpGet("connect")]
        public IActionResult Connect()
        {
            var clientId = _config["Spotify:ClientId"];
            var redirectUri = _config["Spotify:RedirectUri"];
            var scopes = _config["Spotify:Scopes"] ?? "user-top-read";
            var state = Guid.NewGuid().ToString("N");
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = clientId;
            query["response_type"] = "code";
            query["redirect_uri"] = redirectUri;
            query["scope"] = scopes;
            query["state"] = state;
            var url = "https://accounts.spotify.com/authorize?" + query.ToString();
            return Redirect(url);
        }

        // Step 2: handle Spotify redirect, exchange code for tokens, and issue JWT
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Missing code");
            var tokenInfo = await _tokenService.GetAccessTokenAsync(code);
            if (tokenInfo == null) return BadRequest("Token exchange failed");
            // generate JWT
            var jwtSection = _config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key")!);
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);
            var claims = new[] {
                new System.Security.Claims.Claim("access_token", tokenInfo.AccessToken),
                new System.Security.Claims.Claim("refresh_token", tokenInfo.RefreshToken)
            };
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSection.GetValue<int>("ExpiresInMinutes")),
                signingCredentials: creds);
            var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwt);
            // Redirect back to client app with JWT in fragment
            var clientUrl = _config["ClientApp:BaseUrl"] ?? "http://localhost:5173";
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

            var AccessResponse = await _tokenService.GetAccessTokenAsync(code.code); //access token
            if (AccessResponse == null)
            {
                return BadRequest("Failed to retrieve access code");
            };
        
            _logger.LogInformation($"Received token: {code}");
            
            return Ok(new { message = "Token received successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(
            [FromServices] ITokenService TokenService,
            [FromBody] RefreshTokenRequest? request)
        {
            // Use provided refreshToken or fallback to stored one
            var refreshToken = request?.refreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = await TokenService.GetStoredRefreshTokenAsync();
            }
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("No refresh token provided or stored");
            }
            var refreshedToken = await TokenService.RefreshAccessTokenAsync(refreshToken);
            if (refreshedToken == null || string.IsNullOrEmpty(refreshedToken.AccessToken))
            {
                return BadRequest("Failed to refresh token");
            }
            return Ok(new { token = refreshedToken.AccessToken, refreshToken = refreshedToken.RefreshToken });
        }

        public class CodeRequest
        {
            public string? code { get; set; }
        }   

        public class RefreshTokenRequest
        {
            public string? refreshToken { get; set; }
        }
        
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SpotifyTrendsApp.Server.Services;

namespace SpotifyTrendsApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveCode([FromBody] CodeRequest code, [FromServices] ITokenService TokenService)
        {
            _logger.LogDebug(code.code);
            if (code == null || string.IsNullOrEmpty(code.code))
            {
                return BadRequest("Invalid token");
            }

            var AccessResponse = await TokenService.GetAccessTokenAsync(code.code); //access token
            if (AccessResponse == null)
            {
                return BadRequest("Failed to retrieve access code");
            };
        
            _logger.LogInformation($"Received token: {code}");
            
            return Ok(new { message = "Token received successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromServices] ITokenService TokenService)
        {
            var storedRefreshToken = await TokenService.GetStoredRefreshTokenAsync();
            if (string.IsNullOrEmpty(storedRefreshToken))
            {
                return BadRequest("No refresh token found");
            }

            var refreshedToken = await TokenService.RefreshAccessTokenAsync(storedRefreshToken);
            if (string.IsNullOrEmpty(refreshedToken.ToString()))
            {
                return BadRequest("Failed to refresh token");
            }

            return Ok(new { token = refreshedToken });
        }

        public class CodeRequest
        {
            public string code { get; set; }
        }   

        public class RefreshTokenRequest
        {
            public string refreshToken { get; set; }
        }
        
    }
}

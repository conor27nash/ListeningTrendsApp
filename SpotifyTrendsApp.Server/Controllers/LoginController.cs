using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> ReceiveToken([FromBody] AccessToken token, [FromServices] ITokenService TokenService)
        {
            if (token == null || string.IsNullOrEmpty(token.token))
            {
                return BadRequest("Invalid token");
            }

            var AccessResponse = await TokenService.GetAccessTokenAsync(token.token); //access token
            if (AccessResponse == null)
            {
                return BadRequest("Failed to retrieve access token");
            };
        
            _logger.LogInformation($"Received token: {token}");
            
            return Ok(new { message = "Token received successfully" });
        }

        public class AccessToken
        {
            public string token { get; set; }
        }
        
    }
}

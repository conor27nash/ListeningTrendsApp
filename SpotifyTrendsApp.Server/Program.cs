using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SpotifyTrendsApp.Server.Services;
using SpotifyTrendsApp.Server.Middleware;
using System.Net.Http.Headers;

namespace SpotifyTrendsApp.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

            // Validate critical configuration
            var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT_SECRET_KEY environment variable is required");
            
            var spotifyClientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") 
                ?? builder.Configuration["Spotify:ClientId"];
            if (string.IsNullOrEmpty(spotifyClientId))
                throw new InvalidOperationException("SPOTIFY_CLIENT_ID environment variable is required");
                
            var spotifyClientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") 
                ?? builder.Configuration["Spotify:ClientSecret"];
            if (string.IsNullOrEmpty(spotifyClientSecret))
                throw new InvalidOperationException("SPOTIFY_CLIENT_SECRET environment variable is required");

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtKey);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.GetValue<string>("Audience"),
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                };
            });
            builder.Services.AddAuthorization();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache(); 
            builder.Services.AddSingleton<ITokenService, TokenService>();
            
            // Add health checks
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
                .AddCheck("spotify-api", () => 
                {
                    // Basic check that configuration is available
                    var spotifyConfig = builder.Configuration.GetSection("Spotify");
                    return !string.IsNullOrEmpty(spotifyConfig["TokenEndpoint"]) 
                        ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Spotify configuration available")
                        : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Spotify configuration missing");
                });

            builder.Services.AddHttpClient("Spotify", client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
            builder.Services.AddHttpClient("TopItemsService", client =>
            {
                var url = builder.Configuration.GetValue<string>("TopItemsService:BaseUrl") 
                    ?? "http://topitems:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            builder.Services.AddHttpClient("RecentlyPlayedService", client =>
            {
                var url = builder.Configuration.GetValue<string>("RecentlyPlayedService:BaseUrl") 
                    ?? "http://recentlyplayed:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            
            builder.Services.AddHttpClient("TrackService", client =>
            {
                var url = builder.Configuration.GetValue<string>("TrackService:BaseUrl") 
                    ?? "http://trackservice:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            
            builder.Services.AddHttpClient("ArtistService", client =>
            {
                var url = builder.Configuration.GetValue<string>("ArtistService:BaseUrl") 
                    ?? "http://artistservice:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            
            builder.Services.AddHttpClient("UserService", client =>
            {
                var url = builder.Configuration.GetValue<string>("UserService:BaseUrl") 
                    ?? "http://userservice:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            builder.Services.AddHttpClient("AnalyticsService", client =>
            {
                var url = builder.Configuration.GetValue<string>("AnalyticsService:BaseUrl")
                    ?? "http://analyticsservice:5000";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod());
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();

            // Add health check endpoint
            app.MapHealthChecks("/health");

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Urls.Add("http://0.0.0.0:5000");

            app.Run();
        }
    }
}

using SpotifyTrendsApp.Server.Services;

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

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddHttpClient<ITokenService, TokenService>(client =>
            {
                client.BaseAddress = new Uri("https://accounts.spotify.com");
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
            // .AddHttpMessageHandler<TokenAuthHeaderHandler>();

            // builder.Services.AddHttpClient<TopItemsServiceClient>(client =>
            // {
            //     client.BaseAddress = new Uri(builder.Environment.IsDevelopment() ? "http://localhost:5000" : "https://topitemsservice.example.com");
            // });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:5173") // React app URL
                .AllowAnyHeader()
                .AllowAnyMethod());

            // Configure the HTTP request pipeline.
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


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Urls.Add("http://0.0.0.0:5000");

            app.Run();
        }
    }
}

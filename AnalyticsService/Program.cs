using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SpotifyTrends.AnalyticsService.Services.TopItemsService>();
builder.Services.AddScoped<SpotifyTrends.AnalyticsService.Services.AnalyticsService>();

// Add HTTP client for calling TopItems service
builder.Services.AddHttpClient("TopItemsService", client =>
{
    var url = builder.Configuration.GetValue<string>("TopItemsService:BaseUrl") 
        ?? "http://topitems:5000";
    client.BaseAddress = new Uri(url);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["AnalyticsService/AnalyticsService.csproj", "AnalyticsService/"]
COPY ["SpotifyTrendsApp.Common/SpotifyTrendsApp.Common.csproj", "./SpotifyTrendsApp.Common/SpotifyTrendsApp.Common.csproj"]
RUN dotnet restore "AnalyticsService/AnalyticsService.csproj"

# Copy the remaining source code
COPY ["AnalyticsService/", "AnalyticsService/"]
COPY SpotifyTrendsApp.Common/ ./SpotifyTrendsApp.Common/

# Build the application
WORKDIR "/src/AnalyticsService"
RUN dotnet build "AnalyticsService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AnalyticsService.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the ASP.NET Core URLs
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "AnalyticsService.dll"]

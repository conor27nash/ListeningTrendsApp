# Use the official ASP.NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project files and restore dependencies
COPY ["SpotifyTrendsApp.Server/SpotifyTrendsApp.Server.csproj", "./SpotifyTrendsApp.Server.csproj"]
COPY ["SpotifyTrendsApp.Common/SpotifyTrendsApp.Common.csproj", "./SpotifyTrendsApp.Common/SpotifyTrendsApp.Common.csproj"]

RUN dotnet restore "./SpotifyTrendsApp.Server.csproj"

# Copy the entire source code and build the application
COPY SpotifyTrendsApp.Server/ ./SpotifyTrendsApp.Server/
COPY SpotifyTrendsApp.Common/ ./SpotifyTrendsApp.Common/
WORKDIR "/src/SpotifyTrendsApp.Server"
RUN dotnet build "./SpotifyTrendsApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpotifyTrendsApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variable for development
ENV ASPNETCORE_ENVIRONMENT=Development

# Define the entry point for the application
ENTRYPOINT ["dotnet", "SpotifyTrendsApp.Server.dll"]
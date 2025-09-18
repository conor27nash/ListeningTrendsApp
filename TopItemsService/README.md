# TopItemsService

This is a .NET microservice for retrieving a user's top items (artists and tracks) from Spotify's API. It is containerized using Docker and designed to be deployed independently.

## Features
- Fetch top artists and tracks from Spotify.
- Expose RESTful endpoints for integration with other services.
- Containerized for easy deployment.

## Getting Started

### Prerequisites
- .NET SDK 8.0 or later
- Docker
- Spotify API credentials (Client ID and Client Secret)

### Running Locally
1. Clone the repository.
2. Navigate to the project directory.
3. Run the application:
   ```bash
   dotnet run
   ```

### Building the Docker Image
1. Build the Docker image:
   ```bash
   docker build -t top-items-service .
   ```
2. Run the container:
   ```bash
   docker run -p 80:80 top-items-service
   ```

## API Endpoints
- `GET /api/spotify/top-tracks`: Fetch user's top tracks.
- `GET /api/spotify/top-artists`: Fetch user's top artists.

## Environment Variables
- `SPOTIFY_CLIENT_ID`: Your Spotify API Client ID.
- `SPOTIFY_CLIENT_SECRET`: Your Spotify API Client Secret.

## License
This project is licensed under the MIT License.

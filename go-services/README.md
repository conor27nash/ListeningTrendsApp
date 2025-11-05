# ListeningTrendsApp - Go Microservices

This directory contains the Go implementation of the ListeningTrendsApp microservices, converted from C#/.NET.

## Project Structure

```
go-services/
├── cmd/                    # Service entry points (main.go files)
│   ├── user/              # User Service
│   ├── recentlyplayed/    # Recently Played Service
│   ├── topitems/          # Top Items Service
│   ├── artist/            # Artist Service
│   ├── track/             # Track Service
│   ├── analytics/         # Analytics Service
│   └── server/            # Gateway/Server
├── pkg/                   # Shared packages
│   ├── models/            # Shared data models
│   ├── httpclient/        # HTTP client utilities
│   ├── logger/            # Logging configuration
│   ├── config/            # Configuration management
│   └── spotify/           # Spotify API client
├── internal/              # Service-specific internal code
│   ├── user/
│   │   ├── handler/       # HTTP handlers
│   │   └── service/       # Business logic
│   ├── [other services...]/
├── docs/                  # Documentation
├── docker-compose.go.yml  # Docker Compose for Go services
├── Dockerfile.template    # Multi-stage Docker build
├── go.mod                 # Go module definition
└── go.sum                 # Dependency checksums
```

## Prerequisites

- Go 1.21 or higher
- Docker and Docker Compose (for containerized deployment)
- Spotify Developer Account (for OAuth credentials)

## Getting Started

### 1. Install Dependencies

```bash
cd go-services
go mod download
```

### 2. Configure Environment

Copy the example environment file and fill in your Spotify credentials:

```bash
cp .env.example .env
# Edit .env with your Spotify Client ID, Secret, and JWT key
```

### 3. Run Services Locally

#### Run Individual Service
```bash
# User Service
go run cmd/user/main.go

# TopItems Service
go run cmd/topitems/main.go

# ... etc
```

#### Run with Docker Compose
```bash
# Build and run all services
docker-compose -f docker-compose.go.yml up --build

# Run specific service
docker-compose -f docker-compose.go.yml up user-go
```

### 4. Test Services

```bash
# Health check (example for User Service)
curl http://localhost:5000/health

# Get user profile (requires valid Spotify token)
curl -H "Authorization: Bearer <your_token>" http://localhost:5000/api/user/profile
```

## Service Overview

### 1. User Service (Port 5005)
- **Endpoints**: 2
- **Complexity**: ⭐ Simple
- **Dependencies**: Spotify API only
- **Routes**:
  - `GET /api/user/profile` - Get current user profile
  - `GET /api/user/following/artists` - Get followed artists

### 2. Recently Played Service (Port 5002)
- **Endpoints**: 1
- **Complexity**: ⭐ Simple
- **Dependencies**: Spotify API only
- **Routes**:
  - `GET /api/recentlyplayed/recent-tracks` - Get recently played tracks

### 3. Top Items Service (Port 5001)
- **Endpoints**: 2
- **Complexity**: ⭐⭐ Simple-Medium
- **Dependencies**: Spotify API only
- **Routes**:
  - `GET /api/topartists/top-artists/:timeRange` - Get top artists
  - `GET /api/toptracks/top-tracks/:timeRange` - Get top tracks

### 4. Artist Service (Port 5004)
- **Endpoints**: 4
- **Complexity**: ⭐⭐⭐ Medium
- **Dependencies**: Spotify API only
- **Routes**:
  - `GET /api/artists/:id` - Get artist details
  - `GET /api/artists/several` - Get multiple artists
  - `GET /api/artists/:id/top-tracks` - Get artist top tracks
  - `GET /api/artists/:id/albums` - Get artist albums

### 5. Track Service (Port 5003)
- **Endpoints**: 6
- **Complexity**: ⭐⭐⭐⭐ Medium-High
- **Dependencies**: Spotify API only
- **Routes**:
  - `GET /api/tracks/:id` - Get track details
  - `GET /api/tracks/several` - Get multiple tracks
  - `GET /api/tracks/saved` - Get saved tracks
  - `PUT /api/tracks/save` - Save tracks
  - `DELETE /api/tracks/remove` - Remove saved tracks
  - `GET /api/tracks/check-saved` - Check if tracks are saved

### 6. Analytics Service (Port 5006)
- **Endpoints**: 2
- **Complexity**: ⭐⭐⭐⭐⭐ High
- **Dependencies**: TopItems Service (internal)
- **Routes**:
  - `GET /api/analytics/analytics` - Generate analytics
  - `POST /api/analytics/refresh` - Refresh analytics

### 7. Gateway/Server (Port 5000)
- **Endpoints**: 13+
- **Complexity**: ⭐⭐⭐⭐⭐ Very High
- **Dependencies**: All services
- **Routes**:
  - Auth: `/api/login/connect`, `/api/login/callback`, `/api/login/refresh`
  - Proxies to all other services

## Development Workflow

### Adding a New Endpoint

1. **Define the route** in `cmd/<service>/main.go`
2. **Create handler** in `internal/<service>/handler/`
3. **Implement business logic** in `internal/<service>/service/`
4. **Add tests** in `internal/<service>/handler/*_test.go`
5. **Update documentation**

### Example: Adding a new endpoint to User Service

```go
// cmd/user/main.go
router.GET("/api/user/playlists", userHandler.GetPlaylists)

// internal/user/handler/user_handler.go
func (h *UserHandler) GetPlaylists(c *gin.Context) {
    auth := c.GetHeader("Authorization")
    // Implementation...
    c.JSON(200, playlists)
}
```

## Testing

```bash
# Run tests for all services
go test ./...

# Run tests for specific service
go test ./internal/user/...

# Run with coverage
go test -cover ./...

# Run with verbose output
go test -v ./...
```

## Building

### Build Single Service
```bash
# Build user service
go build -o bin/user-service ./cmd/user

# Run the built binary
./bin/user-service
```

### Build All Services
```bash
# Build script
for service in user recentlyplayed topitems artist track analytics server; do
    go build -o bin/${service}-service ./cmd/${service}
done
```

### Build Docker Images
```bash
# Build single service
docker build --build-arg SERVICE=user -f Dockerfile.template -t user-service:latest .

# Build all with docker-compose
docker-compose -f docker-compose.go.yml build
```

## Deployment

### Docker Compose (Recommended for Development)
```bash
docker-compose -f docker-compose.go.yml up -d
```

### Kubernetes (Production)
See `docs/kubernetes-deployment.md` for Kubernetes manifests and deployment guide.

## Monitoring and Logging

All services use structured JSON logging via `logrus`:

```json
{
  "level": "info",
  "msg": "Request processed",
  "service": "user-service",
  "endpoint": "/api/user/profile",
  "status": 200,
  "duration_ms": 145,
  "time": "2025-01-15T10:30:45Z"
}
```

Set log level via `LOG_LEVEL` environment variable: `debug`, `info`, `warn`, `error`

## Troubleshooting

### Service won't start
- Check port availability: `lsof -i :5000`
- Verify environment variables are set
- Check logs: `docker-compose -f docker-compose.go.yml logs user-go`

### 401 Unauthorized
- Verify Spotify token is valid and not expired
- Check Authorization header format: `Bearer <token>`
- Verify JWT configuration (for gateway)

### Service communication issues
- Ensure all dependent services are running
- Check service URLs in environment variables
- Verify network connectivity in Docker: `docker network inspect go-services_default`

## Migration from C# Services

To run Go services alongside existing C# services:

1. **Use different ports** or service names
2. **Update docker-compose** to include both
3. **Configure gateway** to route to Go or C# based on service availability
4. **Gradually migrate** traffic using feature flags or routing rules

## Contributing

1. Follow Go conventions and style guide
2. Write tests for new functionality
3. Update documentation
4. Run `go fmt` and `go vet` before committing
5. Ensure all tests pass: `go test ./...`

## License

Same as parent project

# Go Microservices Implementation Guide

This guide provides step-by-step instructions for implementing each microservice in Go.

## Table of Contents

1. [General Patterns](#general-patterns)
2. [Service Implementation Steps](#service-implementation-steps)
3. [Common Patterns](#common-patterns)
4. [Testing Strategy](#testing-strategy)
5. [Deployment](#deployment)

---

## General Patterns

### Service Structure

Every service follows this structure:

```
cmd/<service>/
  └─ main.go              # Entry point, router setup

internal/<service>/
  ├─ handler/
  │   └─ <service>_handler.go    # HTTP handlers
  └─ service/
      └─ <service>_service.go    # Business logic (if needed)
```

### Handler Pattern

```go
type Handler struct {
    spotifyClient *spotify.Client
    logger        *logrus.Logger
}

func NewHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *Handler {
    return &Handler{
        spotifyClient: spotifyClient,
        logger:        logger,
    }
}

func (h *Handler) HandleEndpoint(c *gin.Context) {
    // 1. Extract and validate input
    auth := c.GetHeader("Authorization")
    if auth == "" {
        c.JSON(401, gin.H{"error": "Authorization required"})
        return
    }

    // 2. Call business logic or external API
    data, statusCode, err := h.spotifyClient.Get("/endpoint", auth)
    if err != nil {
        h.logger.Errorf("Error: %v", err)
        c.JSON(500, gin.H{"error": "Internal server error"})
        return
    }

    // 3. Handle non-200 responses
    if statusCode != 200 {
        c.Data(statusCode, "application/json", data)
        return
    }

    // 4. Parse and return response
    var result interface{}
    if err := json.Unmarshal(data, &result); err != nil {
        c.JSON(500, gin.H{"error": "Failed to parse response"})
        return
    }

    c.JSON(200, result)
}
```

### Main.go Pattern

```go
package main

import (
    "github.com/gin-gonic/gin"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
    "github.com/conor27nash/ListeningTrendsApp/go-services/internal/<service>/handler"
)

func main() {
    // Load configuration
    cfg := config.LoadConfig()

    // Setup logger
    log := logger.NewLogger("<service>-service")

    // Setup Spotify client
    spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)

    // Setup handler
    h := handler.NewHandler(spotifyClient, log)

    // Setup router
    router := gin.Default()

    // Health check
    router.GET("/health", func(c *gin.Context) {
        c.JSON(200, gin.H{"status": "healthy", "service": "<service>"})
    })

    // API routes
    router.GET("/api/<service>/endpoint", h.HandleEndpoint)

    // Start server
    log.Infof("Starting <service> service on port %s", cfg.Port)
    if err := router.Run(":" + cfg.Port); err != nil {
        log.Fatalf("Failed to start server: %v", err)
    }
}
```

---

## Service Implementation Steps

### Phase 1: Simple Services (Start Here)

#### 1. User Service Implementation

**Step 1: Create Handler**

`internal/user/handler/user_handler.go`:

```go
package handler

import (
    "encoding/json"
    "github.com/gin-gonic/gin"
    "github.com/sirupsen/logrus"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
    "strconv"
)

type UserHandler struct {
    spotifyClient *spotify.Client
    logger        *logrus.Logger
}

func NewUserHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *UserHandler {
    return &UserHandler{
        spotifyClient: spotifyClient,
        logger:        logger,
    }
}

// GetProfile handles GET /api/user/profile
func (h *UserHandler) GetProfile(c *gin.Context) {
    auth := c.GetHeader("Authorization")
    if auth == "" {
        h.logger.Warn("GetProfile called without Authorization header")
        c.JSON(401, gin.H{"error": "Bearer token is required"})
        return
    }

    h.logger.Info("Fetching current user profile")

    // Call Spotify API
    data, statusCode, err := h.spotifyClient.Get("/me", auth)
    if err != nil {
        h.logger.Errorf("Failed to fetch user profile: %v", err)
        c.JSON(500, gin.H{"error": "Internal server error"})
        return
    }

    if statusCode != 200 {
        h.logger.Errorf("Spotify API returned status %d", statusCode)
        c.Data(statusCode, "application/json", data)
        return
    }

    // Parse response
    var userProfile interface{}
    if err := json.Unmarshal(data, &userProfile); err != nil {
        h.logger.Errorf("Failed to parse response: %v", err)
        c.JSON(500, gin.H{"error": "Failed to parse response"})
        return
    }

    h.logger.Info("Successfully fetched user profile")
    c.JSON(200, userProfile)
}

// GetFollowedArtists handles GET /api/user/following/artists
func (h *UserHandler) GetFollowedArtists(c *gin.Context) {
    auth := c.GetHeader("Authorization")
    if auth == "" {
        h.logger.Warn("GetFollowedArtists called without Authorization header")
        c.JSON(401, gin.H{"error": "Bearer token is required"})
        return
    }

    // Parse query parameters
    after := c.Query("after")
    limitStr := c.DefaultQuery("limit", "20")
    limit, err := strconv.Atoi(limitStr)
    if err != nil {
        limit = 20
    }

    // Clamp limit to 1-50
    if limit < 1 {
        limit = 1
    }
    if limit > 50 {
        limit = 50
    }

    // Build query string
    endpoint := "/me/following?type=artist&limit=" + strconv.Itoa(limit)
    if after != "" {
        endpoint += "&after=" + after
    }

    h.logger.Infof("Fetching followed artists with limit: %d", limit)

    // Call Spotify API
    data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
    if err != nil {
        h.logger.Errorf("Failed to fetch followed artists: %v", err)
        c.JSON(500, gin.H{"error": "Internal server error"})
        return
    }

    if statusCode != 200 {
        h.logger.Errorf("Spotify API returned status %d", statusCode)
        c.Data(statusCode, "application/json", data)
        return
    }

    // Parse response
    var followedArtists interface{}
    if err := json.Unmarshal(data, &followedArtists); err != nil {
        h.logger.Errorf("Failed to parse response: %v", err)
        c.JSON(500, gin.H{"error": "Failed to parse response"})
        return
    }

    h.logger.Info("Successfully fetched followed artists")
    c.JSON(200, followedArtists)
}
```

**Step 2: Create Main**

`cmd/user/main.go`:

```go
package main

import (
    "github.com/gin-gonic/gin"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
    "github.com/conor27nash/ListeningTrendsApp/go-services/internal/user/handler"
)

func main() {
    // Load configuration
    cfg := config.LoadConfig()

    // Setup logger
    log := logger.NewLogger("user-service")

    // Setup Spotify client
    spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)

    // Setup handler
    userHandler := handler.NewUserHandler(spotifyClient, log)

    // Setup router
    router := gin.Default()

    // Health check
    router.GET("/health", func(c *gin.Context) {
        c.JSON(200, gin.H{
            "status": "healthy",
            "service": "user-service",
        })
    })

    // API routes
    router.GET("/api/user/profile", userHandler.GetProfile)
    router.GET("/api/user/following/artists", userHandler.GetFollowedArtists)

    // Start server
    log.Infof("Starting user service on port %s", cfg.Port)
    if err := router.Run(":" + cfg.Port); err != nil {
        log.Fatalf("Failed to start server: %v", err)
    }
}
```

**Step 3: Test Locally**

```bash
# Run the service
export PORT=5005
export LOG_LEVEL=debug
go run cmd/user/main.go

# Test health endpoint
curl http://localhost:5005/health

# Test with valid token
curl -H "Authorization: Bearer <your_spotify_token>" \
     http://localhost:5005/api/user/profile
```

**Step 4: Build and Deploy**

```bash
# Build binary
go build -o bin/user-service ./cmd/user

# Build Docker image
docker build --build-arg SERVICE=user -f Dockerfile.template -t user-service:latest .

# Run with Docker
docker run -p 5005:5000 \
  -e PORT=5000 \
  -e LOG_LEVEL=info \
  user-service:latest
```

---

#### 2. Recently Played Service Implementation

Similar pattern to User Service. Key differences:

**Query Parameter Handling**:
```go
// Parse optional parameters
limitStr := c.DefaultQuery("limit", "50")
after := c.Query("after")   // optional
before := c.Query("before") // optional

// Validate limit
limit, _ := strconv.Atoi(limitStr)
if limit < 1 {
    limit = 1
}
if limit > 50 {
    limit = 50
}

// Build query string
endpoint := "/me/player/recently-played?limit=" + strconv.Itoa(limit)
if after != "" {
    endpoint += "&after=" + after
}
if before != "" {
    endpoint += "&before=" + before
}
```

---

#### 3. Top Items Service Implementation

**Path Parameter Handling**:
```go
// Extract path parameter
timeRange := c.Param("timeRange")

// Validate time range
validRanges := []string{"short_term", "medium_term", "long_term"}
valid := false
for _, r := range validRanges {
    if r == timeRange {
        valid = true
        break
    }
}
if !valid {
    c.JSON(400, gin.H{"error": "Invalid time range"})
    return
}

// Build endpoint
endpoint := "/me/top/artists?time_range=" + timeRange + "&limit=50"
```

**Routes**:
```go
// Important: Register specific routes BEFORE parameterized routes
router.GET("/api/topartists/top-artists/:timeRange", handler.GetTopArtists)
router.GET("/api/toptracks/top-tracks/:timeRange", handler.GetTopTracks)
```

---

### Phase 2: Medium Services

#### 4. Artist Service Implementation

**Handling CSV IDs**:
```go
// Get comma-separated IDs
idsStr := c.Query("ids")
if idsStr == "" {
    c.JSON(400, gin.H{"error": "Artist IDs are required"})
    return
}

// Split and validate
ids := strings.Split(idsStr, ",")
if len(ids) > 50 {
    c.JSON(400, gin.H{"error": "Maximum 50 artist IDs allowed"})
    return
}

// Build endpoint
endpoint := "/artists?ids=" + idsStr
```

**Route Order Matters**:
```go
// Register specific routes BEFORE parameterized routes
router.GET("/api/artists/several", artistHandler.GetSeveral)
router.GET("/api/artists/:id", artistHandler.GetArtist)
router.GET("/api/artists/:id/top-tracks", artistHandler.GetTopTracks)
router.GET("/api/artists/:id/albums", artistHandler.GetAlbums)
```

---

#### 5. Track Service Implementation

**Request Body Handling (PUT/DELETE)**:
```go
type SaveTracksRequest struct {
    IDs []string `json:"ids" binding:"required"`
}

func (h *TrackHandler) SaveTracks(c *gin.Context) {
    auth := c.GetHeader("Authorization")
    if auth == "" {
        c.JSON(401, gin.H{"error": "Bearer token is required"})
        return
    }

    // Bind JSON body
    var req SaveTracksRequest
    if err := c.ShouldBindJSON(&req); err != nil {
        h.logger.Errorf("Failed to bind request: %v", err)
        c.JSON(400, gin.H{"error": "Invalid request body"})
        return
    }

    // Validate
    if len(req.IDs) == 0 {
        c.JSON(400, gin.H{"error": "Track IDs are required"})
        return
    }
    if len(req.IDs) > 50 {
        c.JSON(400, gin.H{"error": "Maximum 50 track IDs allowed"})
        return
    }

    // Prepare request body
    body := map[string][]string{"ids": req.IDs}

    // Call Spotify API with PUT
    data, statusCode, err := h.spotifyClient.Put("/me/tracks", auth, body)
    if err != nil {
        h.logger.Errorf("Failed to save tracks: %v", err)
        c.JSON(500, gin.H{"error": "Internal server error"})
        return
    }

    if statusCode != 200 {
        c.Data(statusCode, "application/json", data)
        return
    }

    c.JSON(200, gin.H{"message": "Tracks saved successfully"})
}
```

---

### Phase 3: Complex Services

#### 6. Analytics Service Implementation

**Service-to-Service Communication**:

`internal/analytics/service/topitems_client.go`:

```go
package service

import (
    "encoding/json"
    "fmt"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/models"
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/httpclient"
    "strings"
    "time"
)

type TopItemsClient struct {
    httpClient *httpclient.Client
}

func NewTopItemsClient(baseURL string) *TopItemsClient {
    return &TopItemsClient{
        httpClient: httpclient.NewClient(baseURL, 30*time.Second),
    }
}

func (c *TopItemsClient) GetTopTracks(timeRange, auth string) ([]models.Track, error) {
    // Clean bearer prefix
    token := strings.TrimPrefix(auth, "Bearer ")

    headers := map[string]string{
        "Authorization": "Bearer " + token,
    }

    endpoint := fmt.Sprintf("/api/toptracks/top-tracks/%s", timeRange)
    data, statusCode, err := c.httpClient.Get(endpoint, headers)
    if err != nil {
        return nil, err
    }

    if statusCode != 200 {
        return nil, fmt.Errorf("failed to get top tracks: status %d", statusCode)
    }

    // Try parsing as direct array first
    var tracks []models.Track
    if err := json.Unmarshal(data, &tracks); err != nil {
        // Try parsing as {items: [...]}
        var wrapper struct {
            Items []models.Track `json:"items"`
        }
        if err := json.Unmarshal(data, &wrapper); err != nil {
            return nil, fmt.Errorf("failed to parse response: %w", err)
        }
        tracks = wrapper.Items
    }

    return tracks, nil
}
```

**Analytics Generation**:

`internal/analytics/service/analytics_service.go`:

```go
package service

import (
    "github.com/conor27nash/ListeningTrendsApp/go-services/pkg/models"
    "sort"
    "strings"
    "time"
)

type AnalyticsService struct {
    topItemsClient *TopItemsClient
}

func NewAnalyticsService(topItemsClient *TopItemsClient) *AnalyticsService {
    return &AnalyticsService{
        topItemsClient: topItemsClient,
    }
}

func (s *AnalyticsService) GenerateAnalytics(timeRange, auth string) (*models.AnalyticsData, error) {
    // Fetch data from TopItems service
    tracks, err := s.topItemsClient.GetTopTracks(timeRange, auth)
    if err != nil {
        return nil, err
    }

    artists, err := s.topItemsClient.GetTopArtists(timeRange, auth)
    if err != nil {
        return nil, err
    }

    // Generate analytics
    return &models.AnalyticsData{
        AlbumMosaicData:       s.generateAlbumMosaic(tracks),
        TopArtistData:         s.generateTopArtist(tracks),
        ArtistLeaderboardData: s.generateArtistLeaderboard(artists),
        TrackTimelineData:     s.generateTrackTimeline(tracks),
        GenreBubbleData:       s.generateGenreBubbleChart(artists),
    }, nil
}

func (s *AnalyticsService) generateAlbumMosaic(tracks []models.Track) []models.AlbumMosaic {
    albumMap := make(map[string]*models.AlbumMosaic)

    for _, track := range tracks {
        albumName := track.Album.Name
        if _, exists := albumMap[albumName]; !exists {
            artistNames := make([]string, len(track.Artists))
            for i, artist := range track.Artists {
                artistNames[i] = artist.Name
            }

            albumMap[albumName] = &models.AlbumMosaic{
                AlbumName:   albumName,
                ArtistName:  strings.Join(artistNames, ", "),
                SpotifyLink: track.Album.URI,
                Count:       1,
            }
        } else {
            albumMap[albumName].Count++
        }
    }

    // Convert map to slice
    result := make([]models.AlbumMosaic, 0, len(albumMap))
    for _, mosaic := range albumMap {
        result = append(result, *mosaic)
    }

    return result
}

// ... implement other generation methods
```

---

## Common Patterns

### 1. Parameter Validation

```go
// Clamp numeric values
func clampInt(value, min, max int) int {
    if value < min {
        return min
    }
    if value > max {
        return max
    }
    return value
}

// Validate string in list
func contains(list []string, value string) bool {
    for _, item := range list {
        if item == value {
            return true
        }
    }
    return false
}
```

### 2. Error Handling

```go
// Log and return error
func handleError(c *gin.Context, log *logrus.Logger, statusCode int, message string, err error) {
    if err != nil {
        log.Errorf("%s: %v", message, err)
    } else {
        log.Error(message)
    }
    c.JSON(statusCode, gin.H{"error": message})
}
```

### 3. Query String Building

```go
func buildQueryString(params map[string]string) string {
    var parts []string
    for key, value := range params {
        if value != "" {
            parts = append(parts, fmt.Sprintf("%s=%s", key, value))
        }
    }
    if len(parts) == 0 {
        return ""
    }
    return "?" + strings.Join(parts, "&")
}
```

---

## Testing Strategy

### Unit Tests

```go
// internal/user/handler/user_handler_test.go
package handler

import (
    "net/http"
    "net/http/httptest"
    "testing"
    "github.com/gin-gonic/gin"
    "github.com/stretchr/testify/assert"
)

func TestGetProfile_NoAuth(t *testing.T) {
    gin.SetMode(gin.TestMode)

    // Setup
    handler := NewUserHandler(nil, nil)
    router := gin.New()
    router.GET("/api/user/profile", handler.GetProfile)

    // Test
    w := httptest.NewRecorder()
    req, _ := http.NewRequest("GET", "/api/user/profile", nil)
    router.ServeHTTP(w, req)

    // Assert
    assert.Equal(t, 401, w.Code)
}
```

### Integration Tests

Test service-to-service communication with mock servers.

---

## Deployment

See `README.md` for deployment instructions.

---

## Next Steps

1. Implement User Service (simplest)
2. Test thoroughly
3. Move to RecentlyPlayed Service
4. Continue through the phases

Each service should be fully tested before moving to the next.

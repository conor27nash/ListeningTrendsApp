package handler

import (
	"encoding/json"
	"strconv"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

// RecentlyPlayedHandler handles recently played tracks requests
type RecentlyPlayedHandler struct {
	spotifyClient *spotify.Client
	logger        *logrus.Logger
}

// NewRecentlyPlayedHandler creates a new RecentlyPlayedHandler instance
func NewRecentlyPlayedHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *RecentlyPlayedHandler {
	return &RecentlyPlayedHandler{
		spotifyClient: spotifyClient,
		logger:        logger,
	}
}

// GetRecentTracks handles GET /api/recentlyplayed/recent-tracks
// Fetches the user's recently played tracks from Spotify
func (h *RecentlyPlayedHandler) GetRecentTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetRecentTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Parse query parameters
	limitStr := c.DefaultQuery("limit", "50")
	after := c.Query("after")   // optional - unix timestamp in milliseconds
	before := c.Query("before") // optional - unix timestamp in milliseconds

	// Parse and validate limit
	limit, err := strconv.Atoi(limitStr)
	if err != nil {
		limit = 50
	}

	// Clamp limit to 1-50
	if limit < 1 {
		limit = 1
	}
	if limit > 50 {
		limit = 50
	}

	// Build endpoint with query parameters
	endpoint := "/me/player/recently-played?limit=" + strconv.Itoa(limit)
	if after != "" {
		endpoint += "&after=" + after
	}
	if before != "" {
		endpoint += "&before=" + before
	}

	h.logger.Infof("Fetching recently played tracks with limit: %d", limit)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch recently played tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var recentlyPlayed interface{}
	if err := json.Unmarshal(data, &recentlyPlayed); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched recently played tracks")
	c.JSON(200, recentlyPlayed)
}

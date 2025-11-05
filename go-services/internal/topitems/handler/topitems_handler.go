package handler

import (
	"encoding/json"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

// TopItemsHandler handles top items (artists and tracks) requests
type TopItemsHandler struct {
	spotifyClient *spotify.Client
	logger        *logrus.Logger
}

// NewTopItemsHandler creates a new TopItemsHandler instance
func NewTopItemsHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *TopItemsHandler {
	return &TopItemsHandler{
		spotifyClient: spotifyClient,
		logger:        logger,
	}
}

// GetTopArtists handles GET /api/topartists/top-artists/:timeRange
// Fetches the user's top artists from Spotify
func (h *TopItemsHandler) GetTopArtists(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetTopArtists called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

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
		h.logger.Warnf("Invalid time range: %s", timeRange)
		c.JSON(400, gin.H{"error": "Invalid time range. Must be one of: short_term, medium_term, long_term"})
		return
	}

	// Build endpoint
	endpoint := "/me/top/artists?time_range=" + timeRange + "&limit=50"

	h.logger.Infof("Fetching top artists for time range: %s", timeRange)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch top artists: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var topArtists interface{}
	if err := json.Unmarshal(data, &topArtists); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched top artists")
	c.JSON(200, topArtists)
}

// GetTopTracks handles GET /api/toptracks/top-tracks/:timeRange
// Fetches the user's top tracks from Spotify
func (h *TopItemsHandler) GetTopTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetTopTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

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
		h.logger.Warnf("Invalid time range: %s", timeRange)
		c.JSON(400, gin.H{"error": "Invalid time range. Must be one of: short_term, medium_term, long_term"})
		return
	}

	// Build endpoint
	endpoint := "/me/top/tracks?time_range=" + timeRange + "&limit=50"

	h.logger.Infof("Fetching top tracks for time range: %s", timeRange)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch top tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var topTracks interface{}
	if err := json.Unmarshal(data, &topTracks); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched top tracks")
	c.JSON(200, topTracks)
}

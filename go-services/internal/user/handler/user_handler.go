package handler

import (
	"encoding/json"
	"strconv"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

// UserHandler handles user-related requests
type UserHandler struct {
	spotifyClient *spotify.Client
	logger        *logrus.Logger
}

// NewUserHandler creates a new UserHandler instance
func NewUserHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *UserHandler {
	return &UserHandler{
		spotifyClient: spotifyClient,
		logger:        logger,
	}
}

// GetProfile handles GET /api/user/profile
// Fetches the current user's profile from Spotify
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
// Fetches the artists that the current user follows
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

	// Build endpoint
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

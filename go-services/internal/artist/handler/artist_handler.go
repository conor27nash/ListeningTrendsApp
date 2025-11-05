package handler

import (
	"encoding/json"
	"strconv"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

// ArtistHandler handles artist-related requests
type ArtistHandler struct {
	spotifyClient *spotify.Client
	logger        *logrus.Logger
}

// NewArtistHandler creates a new ArtistHandler instance
func NewArtistHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *ArtistHandler {
	return &ArtistHandler{
		spotifyClient: spotifyClient,
		logger:        logger,
	}
}

// GetArtist handles GET /api/artists/:id
// Fetches a single artist's details
func (h *ArtistHandler) GetArtist(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetArtist called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Extract path parameter
	id := c.Param("id")
	if id == "" {
		h.logger.Warn("GetArtist called with empty artist ID")
		c.JSON(400, gin.H{"error": "Artist ID is required"})
		return
	}

	endpoint := "/artists/" + id

	h.logger.Infof("Fetching artist with ID: %s", id)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch artist: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var artist interface{}
	if err := json.Unmarshal(data, &artist); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched artist")
	c.JSON(200, artist)
}

// GetSeveralArtists handles GET /api/artists/several?ids=...
// Fetches multiple artists at once
func (h *ArtistHandler) GetSeveralArtists(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetSeveralArtists called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Get comma-separated IDs
	idsStr := c.Query("ids")
	if idsStr == "" {
		h.logger.Warn("GetSeveralArtists called with empty IDs")
		c.JSON(400, gin.H{"error": "Artist IDs are required"})
		return
	}

	// Split and validate
	ids := strings.Split(idsStr, ",")
	if len(ids) > 50 {
		h.logger.Warnf("GetSeveralArtists called with too many IDs: %d", len(ids))
		c.JSON(400, gin.H{"error": "Maximum 50 artist IDs allowed"})
		return
	}

	endpoint := "/artists?ids=" + idsStr

	h.logger.Infof("Fetching %d artists", len(ids))

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch several artists: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var artists interface{}
	if err := json.Unmarshal(data, &artists); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched several artists")
	c.JSON(200, artists)
}

// GetArtistTopTracks handles GET /api/artists/:id/top-tracks
// Fetches an artist's top tracks
func (h *ArtistHandler) GetArtistTopTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetArtistTopTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Extract path parameter
	id := c.Param("id")
	if id == "" {
		h.logger.Warn("GetArtistTopTracks called with empty artist ID")
		c.JSON(400, gin.H{"error": "Artist ID is required"})
		return
	}

	// Get market parameter (default to US)
	market := c.DefaultQuery("market", "US")

	endpoint := "/artists/" + id + "/top-tracks?market=" + market

	h.logger.Infof("Fetching top tracks for artist: %s in market: %s", id, market)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch artist top tracks: %v", err)
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

	h.logger.Info("Successfully fetched artist top tracks")
	c.JSON(200, topTracks)
}

// GetArtistAlbums handles GET /api/artists/:id/albums
// Fetches an artist's albums with pagination
func (h *ArtistHandler) GetArtistAlbums(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetArtistAlbums called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Extract path parameter
	id := c.Param("id")
	if id == "" {
		h.logger.Warn("GetArtistAlbums called with empty artist ID")
		c.JSON(400, gin.H{"error": "Artist ID is required"})
		return
	}

	// Parse query parameters
	includeGroups := c.Query("include_groups") // optional: album,single,appears_on,compilation
	market := c.Query("market")                // optional
	limitStr := c.DefaultQuery("limit", "20")
	offsetStr := c.DefaultQuery("offset", "0")

	// Parse and validate limit
	limit, err := strconv.Atoi(limitStr)
	if err != nil {
		limit = 20
	}
	if limit < 1 {
		limit = 1
	}
	if limit > 50 {
		limit = 50
	}

	// Parse and validate offset
	offset, err := strconv.Atoi(offsetStr)
	if err != nil {
		offset = 0
	}
	if offset < 0 {
		offset = 0
	}

	// Build endpoint with query parameters
	endpoint := "/artists/" + id + "/albums?limit=" + strconv.Itoa(limit) + "&offset=" + strconv.Itoa(offset)
	if includeGroups != "" {
		endpoint += "&include_groups=" + includeGroups
	}
	if market != "" {
		endpoint += "&market=" + market
	}

	h.logger.Infof("Fetching albums for artist: %s with limit: %d, offset: %d", id, limit, offset)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch artist albums: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var albums interface{}
	if err := json.Unmarshal(data, &albums); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched artist albums")
	c.JSON(200, albums)
}

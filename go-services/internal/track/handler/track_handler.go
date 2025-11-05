package handler

import (
	"encoding/json"
	"strconv"
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

// TrackHandler handles track-related requests
type TrackHandler struct {
	spotifyClient *spotify.Client
	logger        *logrus.Logger
}

// NewTrackHandler creates a new TrackHandler instance
func NewTrackHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *TrackHandler {
	return &TrackHandler{
		spotifyClient: spotifyClient,
		logger:        logger,
	}
}

// SaveTracksRequest represents the request body for save/remove tracks
type SaveTracksRequest struct {
	IDs []string `json:"ids" binding:"required"`
}

// GetTrack handles GET /api/tracks/:id
// Fetches a single track's details
func (h *TrackHandler) GetTrack(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetTrack called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Extract path parameter
	id := c.Param("id")
	if id == "" {
		h.logger.Warn("GetTrack called with empty track ID")
		c.JSON(400, gin.H{"error": "Track ID is required"})
		return
	}

	// Optional market parameter
	market := c.Query("market")

	endpoint := "/tracks/" + id
	if market != "" {
		endpoint += "?market=" + market
	}

	h.logger.Infof("Fetching track with ID: %s", id)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch track: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var track interface{}
	if err := json.Unmarshal(data, &track); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched track")
	c.JSON(200, track)
}

// GetSeveralTracks handles GET /api/tracks/several?ids=...
// Fetches multiple tracks at once
func (h *TrackHandler) GetSeveralTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetSeveralTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Get comma-separated IDs
	idsStr := c.Query("ids")
	if idsStr == "" {
		h.logger.Warn("GetSeveralTracks called with empty IDs")
		c.JSON(400, gin.H{"error": "Track IDs are required"})
		return
	}

	// Split and validate
	ids := strings.Split(idsStr, ",")
	if len(ids) > 50 {
		h.logger.Warnf("GetSeveralTracks called with too many IDs: %d", len(ids))
		c.JSON(400, gin.H{"error": "Maximum 50 track IDs allowed"})
		return
	}

	// Optional market parameter
	market := c.Query("market")

	endpoint := "/tracks?ids=" + idsStr
	if market != "" {
		endpoint += "&market=" + market
	}

	h.logger.Infof("Fetching %d tracks", len(ids))

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch several tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var tracks interface{}
	if err := json.Unmarshal(data, &tracks); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched several tracks")
	c.JSON(200, tracks)
}

// GetSavedTracks handles GET /api/tracks/saved
// Fetches the user's saved tracks with pagination
func (h *TrackHandler) GetSavedTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetSavedTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Parse query parameters
	limitStr := c.DefaultQuery("limit", "20")
	offsetStr := c.DefaultQuery("offset", "0")
	market := c.Query("market") // optional

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

	// Build endpoint
	endpoint := "/me/tracks?limit=" + strconv.Itoa(limit) + "&offset=" + strconv.Itoa(offset)
	if market != "" {
		endpoint += "&market=" + market
	}

	h.logger.Infof("Fetching saved tracks with limit: %d, offset: %d", limit, offset)

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to fetch saved tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response
	var savedTracks interface{}
	if err := json.Unmarshal(data, &savedTracks); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully fetched saved tracks")
	c.JSON(200, savedTracks)
}

// SaveTracks handles PUT /api/tracks/save
// Saves tracks to the user's library
func (h *TrackHandler) SaveTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("SaveTracks called without Authorization header")
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
		h.logger.Warn("SaveTracks called with empty track IDs")
		c.JSON(400, gin.H{"error": "Track IDs are required"})
		return
	}
	if len(req.IDs) > 50 {
		h.logger.Warnf("SaveTracks called with too many IDs: %d", len(req.IDs))
		c.JSON(400, gin.H{"error": "Maximum 50 track IDs allowed"})
		return
	}

	// Prepare request body for Spotify API
	body := map[string][]string{"ids": req.IDs}

	h.logger.Infof("Saving %d tracks", len(req.IDs))

	// Call Spotify API with PUT
	data, statusCode, err := h.spotifyClient.Put("/me/tracks", auth, body)
	if err != nil {
		h.logger.Errorf("Failed to save tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	h.logger.Info("Successfully saved tracks")
	c.JSON(200, gin.H{"message": "Tracks saved successfully"})
}

// RemoveSavedTracks handles DELETE /api/tracks/remove
// Removes saved tracks from the user's library
func (h *TrackHandler) RemoveSavedTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("RemoveSavedTracks called without Authorization header")
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
		h.logger.Warn("RemoveSavedTracks called with empty track IDs")
		c.JSON(400, gin.H{"error": "Track IDs are required"})
		return
	}
	if len(req.IDs) > 50 {
		h.logger.Warnf("RemoveSavedTracks called with too many IDs: %d", len(req.IDs))
		c.JSON(400, gin.H{"error": "Maximum 50 track IDs allowed"})
		return
	}

	// Prepare request body for Spotify API
	body := map[string][]string{"ids": req.IDs}

	h.logger.Infof("Removing %d saved tracks", len(req.IDs))

	// Call Spotify API with DELETE
	data, statusCode, err := h.spotifyClient.Delete("/me/tracks", auth, body)
	if err != nil {
		h.logger.Errorf("Failed to remove saved tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	h.logger.Info("Successfully removed saved tracks")
	c.JSON(200, gin.H{"message": "Tracks removed successfully"})
}

// CheckSavedTracks handles GET /api/tracks/check-saved?ids=...
// Checks if tracks are saved in the user's library
func (h *TrackHandler) CheckSavedTracks(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("CheckSavedTracks called without Authorization header")
		c.JSON(401, gin.H{"error": "Bearer token is required"})
		return
	}

	// Get comma-separated IDs
	idsStr := c.Query("ids")
	if idsStr == "" {
		h.logger.Warn("CheckSavedTracks called with empty IDs")
		c.JSON(400, gin.H{"error": "Track IDs are required"})
		return
	}

	// Split and validate
	ids := strings.Split(idsStr, ",")
	if len(ids) > 50 {
		h.logger.Warnf("CheckSavedTracks called with too many IDs: %d", len(ids))
		c.JSON(400, gin.H{"error": "Maximum 50 track IDs allowed"})
		return
	}

	endpoint := "/me/tracks/contains?ids=" + idsStr

	h.logger.Infof("Checking if %d tracks are saved", len(ids))

	// Call Spotify API
	data, statusCode, err := h.spotifyClient.Get(endpoint, auth)
	if err != nil {
		h.logger.Errorf("Failed to check saved tracks: %v", err)
		c.JSON(500, gin.H{"error": "Internal server error"})
		return
	}

	if statusCode != 200 {
		h.logger.Errorf("Spotify API returned status %d", statusCode)
		c.Data(statusCode, "application/json", data)
		return
	}

	// Parse response (returns boolean array)
	var result interface{}
	if err := json.Unmarshal(data, &result); err != nil {
		h.logger.Errorf("Failed to parse response: %v", err)
		c.JSON(500, gin.H{"error": "Failed to parse response"})
		return
	}

	h.logger.Info("Successfully checked saved status for tracks")
	c.JSON(200, result)
}

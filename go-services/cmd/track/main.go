package main

import (
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/track/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Setup logger
	log := logger.NewLogger("track-service")

	// Setup Spotify client
	spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)

	// Setup handler
	trackHandler := handler.NewTrackHandler(spotifyClient, log)

	// Setup router
	router := gin.Default()

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
			"service": "track-service",
		})
	})

	// API routes
	// IMPORTANT: Register specific routes BEFORE parameterized routes
	router.GET("/api/tracks/several", trackHandler.GetSeveralTracks)
	router.GET("/api/tracks/saved", trackHandler.GetSavedTracks)
	router.GET("/api/tracks/check-saved", trackHandler.CheckSavedTracks)
	router.GET("/api/tracks/:id", trackHandler.GetTrack)
	router.PUT("/api/tracks/save", trackHandler.SaveTracks)
	router.DELETE("/api/tracks/remove", trackHandler.RemoveSavedTracks)

	// Start server
	log.Infof("Starting track service on port %s", cfg.Port)
	if err := router.Run(":" + cfg.Port); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

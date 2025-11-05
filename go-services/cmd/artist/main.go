package main

import (
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/artist/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Setup logger
	log := logger.NewLogger("artist-service")

	// Setup Spotify client
	spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)

	// Setup handler
	artistHandler := handler.NewArtistHandler(spotifyClient, log)

	// Setup router
	router := gin.Default()

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
			"service": "artist-service",
		})
	})

	// API routes
	// IMPORTANT: Register "several" route BEFORE ":id" route to avoid routing conflicts
	router.GET("/api/artists/several", artistHandler.GetSeveralArtists)
	router.GET("/api/artists/:id", artistHandler.GetArtist)
	router.GET("/api/artists/:id/top-tracks", artistHandler.GetArtistTopTracks)
	router.GET("/api/artists/:id/albums", artistHandler.GetArtistAlbums)

	// Start server
	log.Infof("Starting artist service on port %s", cfg.Port)
	if err := router.Run(":" + cfg.Port); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

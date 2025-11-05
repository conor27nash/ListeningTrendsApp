package main

import (
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/topitems/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Setup logger
	log := logger.NewLogger("topitems-service")

	// Setup Spotify client
	spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)

	// Setup handler
	topItemsHandler := handler.NewTopItemsHandler(spotifyClient, log)

	// Setup router
	router := gin.Default()

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
			"service": "topitems-service",
		})
	})

	// API routes
	router.GET("/api/topartists/top-artists/:timeRange", topItemsHandler.GetTopArtists)
	router.GET("/api/toptracks/top-tracks/:timeRange", topItemsHandler.GetTopTracks)

	// Start server
	log.Infof("Starting top items service on port %s", cfg.Port)
	if err := router.Run(":" + cfg.Port); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

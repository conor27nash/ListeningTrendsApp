package main

import (
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/user/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/spotify"
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

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
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

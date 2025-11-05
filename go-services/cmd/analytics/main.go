package main

import (
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/analytics/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/analytics/service"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Setup logger
	log := logger.NewLogger("analytics-service")

	// Setup TopItems client (calls TopItems service, not Spotify directly)
	topItemsClient := service.NewTopItemsClient(cfg.TopItemsServiceURL)

	// Setup analytics service
	analyticsService := service.NewAnalyticsService(topItemsClient, log)

	// Setup handler
	analyticsHandler := handler.NewAnalyticsHandler(analyticsService, log)

	// Setup router
	router := gin.Default()

	// Health check endpoint
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
			"service": "analytics-service",
		})
	})

	// API routes
	router.GET("/api/analytics/analytics", analyticsHandler.GetAnalytics)
	router.POST("/api/analytics/refresh", analyticsHandler.RefreshAnalytics)

	// Start server
	log.Infof("Starting analytics service on port %s", cfg.Port)
	log.Infof("TopItems service URL: %s", cfg.TopItemsServiceURL)
	if err := router.Run(":" + cfg.Port); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

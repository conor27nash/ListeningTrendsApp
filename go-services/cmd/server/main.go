package main

import (
	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/handler"
	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/middleware"
	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/service"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/logger"
)

func main() {
	// Load configuration
	cfg := config.LoadConfig()

	// Setup logger
	log := logger.NewLogger("server-gateway")

	// Validate JWT key
	if cfg.JWTKey == "" {
		log.Fatal("JWT_KEY environment variable is required")
	}
	jwtKey := []byte(cfg.JWTKey)

	// Setup token service
	tokenService := service.NewTokenService(cfg, log)

	// Setup handlers
	authHandler := handler.NewAuthHandler(cfg, tokenService, jwtKey, log)
	proxyHandler := handler.NewProxyHandler(cfg, log)

	// Setup router
	router := gin.Default()

	// CORS configuration
	corsConfig := cors.Config{
		AllowOrigins:     []string{cfg.ClientAppBaseURL, "http://localhost:5173"},
		AllowMethods:     []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowHeaders:     []string{"Origin", "Content-Type", "Authorization"},
		ExposeHeaders:    []string{"Content-Length"},
		AllowCredentials: true,
	}
	router.Use(cors.New(corsConfig))

	// Health check
	router.GET("/health", func(c *gin.Context) {
		c.JSON(200, gin.H{
			"status":  "healthy",
			"service": "server-gateway",
		})
	})

	// Public routes (no JWT required)
	authGroup := router.Group("/api/login")
	{
		authGroup.GET("/connect", authHandler.Connect)
		authGroup.GET("/callback", authHandler.Callback)
	}

	// Protected routes (JWT required)
	protected := router.Group("/")
	protected.Use(middleware.JWTMiddleware(jwtKey, log))
	{
		// Auth routes
		protected.POST("/api/login/refresh", authHandler.RefreshToken)

		// Proxy routes to microservices
		// User Service
		protected.GET("/api/userproxy/profile",
			proxyHandler.ProxyToService("user", proxyHandler.GetServiceURL("user")))
		protected.GET("/api/userproxy/following/artists",
			proxyHandler.ProxyToService("user", proxyHandler.GetServiceURL("user")))

		// Recently Played Service
		protected.GET("/api/recentlyplayedproxy/recent-tracks",
			proxyHandler.ProxyToService("recentlyplayed", proxyHandler.GetServiceURL("recentlyplayed")))

		// Top Items Service - Artists
		protected.GET("/api/topartistsproxy/top-artists/:timeRange",
			proxyHandler.ProxyToService("topitems", proxyHandler.GetServiceURL("topitems")))

		// Top Items Service - Tracks
		protected.GET("/api/toptracksproxy/top-tracks/:timeRange",
			proxyHandler.ProxyToService("topitems", proxyHandler.GetServiceURL("topitems")))

		// Artist Service
		protected.GET("/api/artistsproxy/several",
			proxyHandler.ProxyToService("artist", proxyHandler.GetServiceURL("artist")))
		protected.GET("/api/artistsproxy/:id",
			proxyHandler.ProxyToService("artist", proxyHandler.GetServiceURL("artist")))
		protected.GET("/api/artistsproxy/:id/top-tracks",
			proxyHandler.ProxyToService("artist", proxyHandler.GetServiceURL("artist")))
		protected.GET("/api/artistsproxy/:id/albums",
			proxyHandler.ProxyToService("artist", proxyHandler.GetServiceURL("artist")))

		// Track Service
		protected.GET("/api/tracksproxy/several",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))
		protected.GET("/api/tracksproxy/saved",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))
		protected.GET("/api/tracksproxy/check-saved",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))
		protected.GET("/api/tracksproxy/:id",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))
		protected.PUT("/api/tracksproxy/save",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))
		protected.DELETE("/api/tracksproxy/remove",
			proxyHandler.ProxyToService("track", proxyHandler.GetServiceURL("track")))

		// Analytics Service
		protected.GET("/api/analyticsproxy/analytics",
			proxyHandler.ProxyToService("analytics", proxyHandler.GetServiceURL("analytics")))
		protected.POST("/api/analyticsproxy/refresh",
			proxyHandler.ProxyToService("analytics", proxyHandler.GetServiceURL("analytics")))
	}

	// Static file serving for React app (optional - if serving frontend)
	// router.Static("/assets", "./wwwroot/assets")
	// router.NoRoute(func(c *gin.Context) {
	//     c.File("./wwwroot/index.html")
	// })

	// Start server
	log.Infof("Starting server/gateway on port %s", cfg.Port)
	log.Infof("Client app URL: %s", cfg.ClientAppBaseURL)
	log.Infof("Service URLs:")
	log.Infof("  - User: %s", cfg.UserServiceURL)
	log.Infof("  - RecentlyPlayed: %s", cfg.RecentlyPlayedServiceURL)
	log.Infof("  - TopItems: %s", cfg.TopItemsServiceURL)
	log.Infof("  - Artist: %s", cfg.ArtistServiceURL)
	log.Infof("  - Track: %s", cfg.TrackServiceURL)
	log.Infof("  - Analytics: %s", cfg.AnalyticsServiceURL)

	if err := router.Run(":" + cfg.Port); err != nil {
		log.Fatalf("Failed to start server: %v", err)
	}
}

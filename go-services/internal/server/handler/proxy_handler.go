package handler

import (
	"bytes"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
)

// ProxyHandler handles proxying requests to microservices
type ProxyHandler struct {
	config     *config.Config
	httpClient *http.Client
	logger     *logrus.Logger
}

// NewProxyHandler creates a new ProxyHandler instance
func NewProxyHandler(cfg *config.Config, logger *logrus.Logger) *ProxyHandler {
	return &ProxyHandler{
		config: cfg,
		httpClient: &http.Client{
			Timeout: 30 * time.Second,
		},
		logger: logger,
	}
}

// ProxyToService proxies a request to a specific microservice
func (h *ProxyHandler) ProxyToService(serviceName, serviceURL string) gin.HandlerFunc {
	return func(c *gin.Context) {
		// Extract Spotify token from context (set by JWT middleware)
		spotifyToken, exists := c.Get("spotify_token")
		if !exists {
			h.logger.Warn("No Spotify token found in context")
			c.JSON(401, gin.H{"error": "Unauthorized"})
			return
		}

		token, ok := spotifyToken.(string)
		if !ok {
			h.logger.Warn("Invalid Spotify token in context")
			c.JSON(401, gin.H{"error": "Invalid token"})
			return
		}

		// Build target URL
		targetURL := serviceURL + c.Request.URL.Path

		// Add query parameters if present
		if c.Request.URL.RawQuery != "" {
			targetURL += "?" + c.Request.URL.RawQuery
		}

		h.logger.Debugf("Proxying %s request to %s: %s", c.Request.Method, serviceName, targetURL)

		// Read request body (if present)
		var bodyBytes []byte
		if c.Request.Body != nil {
			bodyBytes, _ = io.ReadAll(c.Request.Body)
			c.Request.Body = io.NopCloser(bytes.NewBuffer(bodyBytes)) // Reset body for potential re-reading
		}

		// Create new request
		req, err := http.NewRequest(c.Request.Method, targetURL, bytes.NewReader(bodyBytes))
		if err != nil {
			h.logger.Errorf("Failed to create proxy request: %v", err)
			c.JSON(500, gin.H{"error": "Internal server error"})
			return
		}

		// Forward Spotify token in Authorization header
		req.Header.Set("Authorization", "Bearer "+token)

		// Forward Content-Type if present
		if contentType := c.GetHeader("Content-Type"); contentType != "" {
			req.Header.Set("Content-Type", contentType)
		}

		// Send request to microservice
		resp, err := h.httpClient.Do(req)
		if err != nil {
			h.logger.Errorf("Proxy request failed: %v", err)
			c.JSON(503, gin.H{"error": fmt.Sprintf("%s service unavailable", serviceName)})
			return
		}
		defer resp.Body.Close()

		// Read response
		respBody, err := io.ReadAll(resp.Body)
		if err != nil {
			h.logger.Errorf("Failed to read proxy response: %v", err)
			c.JSON(500, gin.H{"error": "Failed to read service response"})
			return
		}

		// Forward response
		h.logger.Debugf("Proxy response from %s: status %d", serviceName, resp.StatusCode)

		// Copy response headers
		for key, values := range resp.Header {
			for _, value := range values {
				c.Header(key, value)
			}
		}

		// Send response
		c.Data(resp.StatusCode, resp.Header.Get("Content-Type"), respBody)
	}
}

// GetServiceURL returns the configured URL for a service
func (h *ProxyHandler) GetServiceURL(serviceName string) string {
	switch serviceName {
	case "topitems":
		return h.config.TopItemsServiceURL
	case "recentlyplayed":
		return h.config.RecentlyPlayedServiceURL
	case "artist":
		return h.config.ArtistServiceURL
	case "track":
		return h.config.TrackServiceURL
	case "user":
		return h.config.UserServiceURL
	case "analytics":
		return h.config.AnalyticsServiceURL
	default:
		return ""
	}
}

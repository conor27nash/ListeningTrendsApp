package handler

import (
	"github.com/gin-gonic/gin"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/analytics/service"
)

// AnalyticsHandler handles analytics requests
type AnalyticsHandler struct {
	analyticsService *service.AnalyticsService
	logger           *logrus.Logger
}

// NewAnalyticsHandler creates a new AnalyticsHandler instance
func NewAnalyticsHandler(analyticsService *service.AnalyticsService, logger *logrus.Logger) *AnalyticsHandler {
	return &AnalyticsHandler{
		analyticsService: analyticsService,
		logger:           logger,
	}
}

// GetAnalytics handles GET /api/analytics/analytics
// Generates analytics data for the current user
func (h *AnalyticsHandler) GetAnalytics(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("GetAnalytics called without Authorization header")
		c.JSON(401, gin.H{"error": "Authorization header is required"})
		return
	}

	// Get time range parameter (default to medium_term)
	timeRange := c.DefaultQuery("timeRange", "medium_term")

	// Validate time range
	validRanges := []string{"short_term", "medium_term", "long_term"}
	valid := false
	for _, r := range validRanges {
		if r == timeRange {
			valid = true
			break
		}
	}
	if !valid {
		h.logger.Warnf("Invalid time range: %s", timeRange)
		timeRange = "medium_term"
	}

	h.logger.Infof("Getting analytics data for timeRange: %s", timeRange)

	// Call AnalyticsService to generate analytics
	analyticsData, err := h.analyticsService.GenerateAnalytics(timeRange, auth)
	if err != nil {
		h.logger.Errorf("Error generating analytics: %v", err)
		c.JSON(500, gin.H{"message": "Error generating analytics", "error": err.Error()})
		return
	}

	h.logger.Info("Successfully generated analytics")
	c.JSON(200, analyticsData)
}

// RefreshAnalytics handles POST /api/analytics/refresh
// Placeholder for refreshing analytics data
func (h *AnalyticsHandler) RefreshAnalytics(c *gin.Context) {
	auth := c.GetHeader("Authorization")
	if auth == "" {
		h.logger.Warn("RefreshAnalytics called without Authorization header")
		c.JSON(401, gin.H{"error": "Authorization header is required"})
		return
	}

	h.logger.Info("Refreshing analytics data")

	// For now, just return a success message
	// In the future, this could trigger cache invalidation or background refresh
	c.JSON(200, gin.H{
		"message":   "Analytics data refreshed successfully",
		"timestamp": "now",
		"status":    "refreshed",
	})
}

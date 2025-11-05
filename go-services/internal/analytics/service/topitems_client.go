package service

import (
	"encoding/json"
	"fmt"
	"strings"
	"time"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/httpclient"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/models"
)

// TopItemsClient handles communication with the TopItems service
type TopItemsClient struct {
	httpClient *httpclient.Client
}

// NewTopItemsClient creates a new TopItemsClient instance
func NewTopItemsClient(baseURL string) *TopItemsClient {
	return &TopItemsClient{
		httpClient: httpclient.NewClient(baseURL, 30*time.Second),
	}
}

// GetTopTracks fetches top tracks from the TopItems service
func (c *TopItemsClient) GetTopTracks(timeRange, auth string) ([]models.Track, error) {
	// Clean bearer prefix if present
	token := strings.TrimPrefix(auth, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	endpoint := fmt.Sprintf("/api/toptracks/top-tracks/%s", timeRange)
	data, statusCode, err := c.httpClient.Get(endpoint, headers)
	if err != nil {
		return nil, fmt.Errorf("failed to call TopItems service: %w", err)
	}

	if statusCode != 200 {
		return nil, fmt.Errorf("TopItems service returned status %d: %s", statusCode, string(data))
	}

	// Try parsing as direct array first
	var tracks []models.Track
	if err := json.Unmarshal(data, &tracks); err != nil {
		// Try parsing as {items: [...]} wrapper format
		var wrapper struct {
			Items []models.Track `json:"items"`
		}
		if err := json.Unmarshal(data, &wrapper); err != nil {
			return nil, fmt.Errorf("failed to parse tracks response: %w", err)
		}
		tracks = wrapper.Items
	}

	return tracks, nil
}

// GetTopArtists fetches top artists from the TopItems service
func (c *TopItemsClient) GetTopArtists(timeRange, auth string) ([]models.Artist, error) {
	// Clean bearer prefix if present
	token := strings.TrimPrefix(auth, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	endpoint := fmt.Sprintf("/api/topartists/top-artists/%s", timeRange)
	data, statusCode, err := c.httpClient.Get(endpoint, headers)
	if err != nil {
		return nil, fmt.Errorf("failed to call TopItems service: %w", err)
	}

	if statusCode != 200 {
		return nil, fmt.Errorf("TopItems service returned status %d: %s", statusCode, string(data))
	}

	// Try parsing as direct array first
	var artists []models.Artist
	if err := json.Unmarshal(data, &artists); err != nil {
		// Try parsing as {items: [...]} wrapper format
		var wrapper struct {
			Items []models.Artist `json:"items"`
		}
		if err := json.Unmarshal(data, &wrapper); err != nil {
			return nil, fmt.Errorf("failed to parse artists response: %w", err)
		}
		artists = wrapper.Items
	}

	return artists, nil
}

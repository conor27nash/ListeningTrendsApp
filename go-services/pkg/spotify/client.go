package spotify

import (
	"encoding/json"
	"fmt"
	"strings"
	"time"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/httpclient"
)

// Client wraps Spotify API calls
type Client struct {
	httpClient *httpclient.Client
}

// NewClient creates a new Spotify API client
func NewClient(baseURL string) *Client {
	return &Client{
		httpClient: httpclient.NewClient(baseURL, 30*time.Second),
	}
}

// Get performs a GET request to Spotify API with authorization
func (c *Client) Get(endpoint string, authorization string) ([]byte, int, error) {
	// Clean Bearer prefix if present
	token := strings.TrimPrefix(authorization, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	return c.httpClient.Get(endpoint, headers)
}

// Post performs a POST request to Spotify API with authorization
func (c *Client) Post(endpoint string, authorization string, body interface{}) ([]byte, int, error) {
	token := strings.TrimPrefix(authorization, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	return c.httpClient.Post(endpoint, headers, body)
}

// Put performs a PUT request to Spotify API with authorization
func (c *Client) Put(endpoint string, authorization string, body interface{}) ([]byte, int, error) {
	token := strings.TrimPrefix(authorization, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	return c.httpClient.Put(endpoint, headers, body)
}

// Delete performs a DELETE request to Spotify API with authorization
func (c *Client) Delete(endpoint string, authorization string, body interface{}) ([]byte, int, error) {
	token := strings.TrimPrefix(authorization, "Bearer ")

	headers := map[string]string{
		"Authorization": "Bearer " + token,
	}

	return c.httpClient.Delete(endpoint, headers, body)
}

// DecodeResponse decodes JSON response into target struct
func DecodeResponse(data []byte, target interface{}) error {
	if err := json.Unmarshal(data, target); err != nil {
		return fmt.Errorf("failed to decode response: %w", err)
	}
	return nil
}

// HandleSpotifyError checks status code and returns appropriate error
func HandleSpotifyError(statusCode int, body []byte) error {
	if statusCode >= 200 && statusCode < 300 {
		return nil
	}

	var errResp struct {
		Error struct {
			Status  int    `json:"status"`
			Message string `json:"message"`
		} `json:"error"`
	}

	if err := json.Unmarshal(body, &errResp); err == nil {
		return fmt.Errorf("spotify API error: %d - %s", errResp.Error.Status, errResp.Error.Message)
	}

	return fmt.Errorf("spotify API error: %d - %s", statusCode, string(body))
}

package service

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"strings"

	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/model"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
)

// TokenService handles Spotify OAuth token operations
type TokenService struct {
	config     *config.Config
	httpClient *http.Client
	logger     *logrus.Logger
}

// NewTokenService creates a new TokenService instance
func NewTokenService(cfg *config.Config, logger *logrus.Logger) *TokenService {
	return &TokenService{
		config:     cfg,
		httpClient: &http.Client{},
		logger:     logger,
	}
}

// GetAccessToken exchanges an authorization code for access and refresh tokens
func (s *TokenService) GetAccessToken(code string) (*model.TokenInfo, error) {
	// Prepare form data
	data := url.Values{}
	data.Set("grant_type", "authorization_code")
	data.Set("code", code)
	data.Set("redirect_uri", s.config.RedirectURI)
	data.Set("client_id", s.config.SpotifyClientID)
	data.Set("client_secret", s.config.SpotifyClientSecret)

	// Create request
	req, err := http.NewRequest("POST", "https://accounts.spotify.com/api/token", strings.NewReader(data.Encode()))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	req.Header.Set("Content-Type", "application/x-www-form-urlencoded")

	// Send request
	s.logger.Debug("Exchanging authorization code for tokens")
	resp, err := s.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to send request: %w", err)
	}
	defer resp.Body.Close()

	// Read response
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %w", err)
	}

	if resp.StatusCode != 200 {
		return nil, fmt.Errorf("token exchange failed with status %d: %s", resp.StatusCode, string(body))
	}

	// Parse response
	var tokenResponse model.OAuthTokenResponse
	if err := json.Unmarshal(body, &tokenResponse); err != nil {
		return nil, fmt.Errorf("failed to parse token response: %w", err)
	}

	s.logger.Info("Successfully exchanged code for tokens")

	return &model.TokenInfo{
		AccessToken:  tokenResponse.AccessToken,
		RefreshToken: tokenResponse.RefreshToken,
		ExpiresIn:    tokenResponse.ExpiresIn,
		TokenType:    tokenResponse.TokenType,
		Scope:        tokenResponse.Scope,
	}, nil
}

// RefreshAccessToken refreshes the access token using a refresh token
func (s *TokenService) RefreshAccessToken(refreshToken string) (*model.TokenInfo, error) {
	// Prepare form data
	data := url.Values{}
	data.Set("grant_type", "refresh_token")
	data.Set("refresh_token", refreshToken)
	data.Set("client_id", s.config.SpotifyClientID)
	data.Set("client_secret", s.config.SpotifyClientSecret)

	// Create request
	req, err := http.NewRequest("POST", "https://accounts.spotify.com/api/token", strings.NewReader(data.Encode()))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	req.Header.Set("Content-Type", "application/x-www-form-urlencoded")

	// Send request
	s.logger.Debug("Refreshing access token")
	resp, err := s.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("failed to send request: %w", err)
	}
	defer resp.Body.Close()

	// Read response
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %w", err)
	}

	if resp.StatusCode != 200 {
		return nil, fmt.Errorf("token refresh failed with status %d: %s", resp.StatusCode, string(body))
	}

	// Parse response
	var tokenResponse model.OAuthTokenResponse
	if err := json.Unmarshal(body, &tokenResponse); err != nil {
		return nil, fmt.Errorf("failed to parse token response: %w", err)
	}

	s.logger.Info("Successfully refreshed access token")

	// Refresh token might not be returned, so use the old one if not present
	if tokenResponse.RefreshToken == "" {
		tokenResponse.RefreshToken = refreshToken
	}

	return &model.TokenInfo{
		AccessToken:  tokenResponse.AccessToken,
		RefreshToken: tokenResponse.RefreshToken,
		ExpiresIn:    tokenResponse.ExpiresIn,
		TokenType:    tokenResponse.TokenType,
		Scope:        tokenResponse.Scope,
	}, nil
}

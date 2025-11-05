package handler

import (
	"fmt"
	"net/url"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/google/uuid"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/model"
	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/service"
	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/config"
)

// AuthHandler handles authentication and OAuth flow
type AuthHandler struct {
	config       *config.Config
	tokenService *service.TokenService
	jwtKey       []byte
	logger       *logrus.Logger
}

// NewAuthHandler creates a new AuthHandler instance
func NewAuthHandler(cfg *config.Config, tokenService *service.TokenService, jwtKey []byte, logger *logrus.Logger) *AuthHandler {
	return &AuthHandler{
		config:       cfg,
		tokenService: tokenService,
		jwtKey:       jwtKey,
		logger:       logger,
	}
}

// Connect handles GET /api/login/connect
// Initiates the Spotify OAuth flow by redirecting to Spotify
func (h *AuthHandler) Connect(c *gin.Context) {
	// Generate random state for CSRF protection
	state := uuid.New().String()

	// Build authorization URL
	params := url.Values{}
	params.Set("client_id", h.config.SpotifyClientID)
	params.Set("response_type", "code")
	params.Set("redirect_uri", h.config.RedirectURI)
	params.Set("scope", h.config.Scopes)
	params.Set("state", state)
	params.Set("show_dialog", "true")

	authURL := "https://accounts.spotify.com/authorize?" + params.Encode()

	h.logger.Infof("Redirecting to Spotify authorization URL with state: %s", state)

	// Redirect to Spotify
	c.Redirect(302, authURL)
}

// Callback handles GET /api/login/callback
// Handles the OAuth callback from Spotify, exchanges code for tokens, and generates JWT
func (h *AuthHandler) Callback(c *gin.Context) {
	// Get authorization code and state
	code := c.Query("code")
	state := c.Query("state")

	if code == "" {
		h.logger.Warn("Callback called without code")
		c.JSON(400, gin.H{"error": "Missing code"})
		return
	}

	h.logger.Infof("Received OAuth callback with state: %s", state)

	// Exchange code for Spotify access token
	spotifyTokens, err := h.tokenService.GetAccessToken(code)
	if err != nil {
		h.logger.Errorf("Token exchange failed: %v", err)
		c.JSON(400, gin.H{"error": "Token exchange failed"})
		return
	}

	// Generate JWT containing Spotify tokens
	jwt Token := h.generateJWT(spotifyTokens)

	// Redirect to client app with JWT
	clientURL := fmt.Sprintf("%s/?token=%s", h.config.ClientAppBaseURL, jwtToken)

	h.logger.Info("Successfully generated JWT, redirecting to client app")
	c.Redirect(302, clientURL)
}

// RefreshToken handles POST /api/login/refresh
// Refreshes the JWT using the Spotify refresh token
func (h *AuthHandler) RefreshToken(c *gin.Context) {
	// Extract refresh token from claims (set by JWT middleware)
	refreshTokenVal, exists := c.Get("refresh_token")
	if !exists {
		h.logger.Warn("No refresh token found in claims")
		c.JSON(401, gin.H{"error": "No Spotify refresh token available"})
		return
	}

	refreshToken, ok := refreshTokenVal.(string)
	if !ok || refreshToken == "" {
		h.logger.Warn("Invalid refresh token in claims")
		c.JSON(401, gin.H{"error": "Invalid refresh token"})
		return
	}

	h.logger.Debug("Refreshing Spotify access token")

	// Get new Spotify access token
	updatedTokens, err := h.tokenService.RefreshAccessToken(refreshToken)
	if err != nil {
		h.logger.Errorf("Token refresh failed: %v", err)
		c.JSON(400, gin.H{"error": "Token refresh failed"})
		return
	}

	// Generate new JWT with updated tokens
	jwtToken := h.generateJWT(updatedTokens)

	h.logger.Info("Successfully refreshed and generated new JWT")
	c.JSON(200, gin.H{"token": jwtToken})
}

// generateJWT creates a JWT containing Spotify tokens
func (h *AuthHandler) generateJWT(tokenInfo *model.TokenInfo) string {
	// Create claims
	claims := &model.Claims{
		AccessToken:  tokenInfo.AccessToken,
		RefreshToken: tokenInfo.RefreshToken,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(time.Duration(h.config.JWTExpiresInMinutes) * time.Minute)),
			Issuer:    h.config.JWTIssuer,
			Audience:  jwt.ClaimStrings{h.config.JWTAudience},
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	}

	// Create token
	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	// Sign token
	tokenString, err := token.SignedString(h.jwtKey)
	if err != nil {
		h.logger.Errorf("Failed to sign JWT: %v", err)
		return ""
	}

	return tokenString
}

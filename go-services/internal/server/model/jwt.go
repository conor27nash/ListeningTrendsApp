package model

import "github.com/golang-jwt/jwt/v5"

// Claims represents JWT claims with Spotify tokens
type Claims struct {
	AccessToken  string `json:"access_token"`
	RefreshToken string `json:"refresh_token"`
	jwt.RegisteredClaims
}

// TokenInfo represents Spotify OAuth token response
type TokenInfo struct {
	AccessToken  string `json:"access_token"`
	RefreshToken string `json:"refresh_token"`
	ExpiresIn    int    `json:"expires_in"`
	TokenType    string `json:"token_type"`
	Scope        string `json:"scope"`
}

// OAuthTokenResponse represents the response from Spotify OAuth token endpoint
type OAuthTokenResponse struct {
	AccessToken  string `json:"access_token"`
	TokenType    string `json:"token_type"`
	ExpiresIn    int    `json:"expires_in"`
	RefreshToken string `json:"refresh_token"`
	Scope        string `json:"scope"`
}

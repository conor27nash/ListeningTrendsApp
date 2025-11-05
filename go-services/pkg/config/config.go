package config

import (
	"os"
	"strconv"
)

// Config holds application configuration
type Config struct {
	// Service settings
	Port         string
	Environment  string

	// Spotify API
	SpotifyBaseURL string

	// Service URLs (for gateway and analytics)
	TopItemsServiceURL      string
	RecentlyPlayedServiceURL string
	ArtistServiceURL        string
	TrackServiceURL         string
	UserServiceURL          string
	AnalyticsServiceURL     string

	// OAuth settings (for gateway)
	SpotifyClientID     string
	SpotifyClientSecret string
	RedirectURI         string
	Scopes              string

	// JWT settings (for gateway)
	JWTKey             string
	JWTIssuer          string
	JWTAudience        string
	JWTExpiresInMinutes int

	// Client app
	ClientAppBaseURL string

	// Logging
	LogLevel string
}

// LoadConfig loads configuration from environment variables
func LoadConfig() *Config {
	return &Config{
		Port:                     getEnv("PORT", "5000"),
		Environment:              getEnv("ASPNETCORE_ENVIRONMENT", "Development"),
		SpotifyBaseURL:           getEnv("SPOTIFY_BASE_URL", "https://api.spotify.com/v1"),
		TopItemsServiceURL:       getEnv("TOPITEMS_SERVICE_URL", "http://topitems:5000"),
		RecentlyPlayedServiceURL: getEnv("RECENTLYPLAYED_SERVICE_URL", "http://recentlyplayed:5000"),
		ArtistServiceURL:         getEnv("ARTIST_SERVICE_URL", "http://artistservice:5000"),
		TrackServiceURL:          getEnv("TRACK_SERVICE_URL", "http://trackservice:5000"),
		UserServiceURL:           getEnv("USER_SERVICE_URL", "http://userservice:5000"),
		AnalyticsServiceURL:      getEnv("ANALYTICS_SERVICE_URL", "http://analyticsservice:5000"),
		SpotifyClientID:          getEnv("SPOTIFY_CLIENT_ID", ""),
		SpotifyClientSecret:      getEnv("SPOTIFY_CLIENT_SECRET", ""),
		RedirectURI:              getEnv("SPOTIFY_REDIRECT_URI", "http://localhost:5000/api/login/callback"),
		Scopes:                   getEnv("SPOTIFY_SCOPES", "user-top-read user-read-recently-played user-read-private user-read-email user-follow-read user-library-read user-library-modify"),
		JWTKey:                   getEnv("JWT_KEY", ""),
		JWTIssuer:                getEnv("JWT_ISSUER", "SpotifyTrendsApp"),
		JWTAudience:              getEnv("JWT_AUDIENCE", "SpotifyTrendsAppClients"),
		JWTExpiresInMinutes:      getEnvInt("JWT_EXPIRES_IN_MINUTES", 60),
		ClientAppBaseURL:         getEnv("CLIENT_APP_BASE_URL", "http://localhost:5173"),
		LogLevel:                 getEnv("LOG_LEVEL", "info"),
	}
}

// getEnv gets an environment variable or returns a default value
func getEnv(key, defaultValue string) string {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	return value
}

// getEnvInt gets an environment variable as int or returns a default value
func getEnvInt(key string, defaultValue int) int {
	value := os.Getenv(key)
	if value == "" {
		return defaultValue
	}
	intValue, err := strconv.Atoi(value)
	if err != nil {
		return defaultValue
	}
	return intValue
}

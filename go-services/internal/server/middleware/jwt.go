package middleware

import (
	"strings"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/internal/server/model"
)

// JWTMiddleware validates JWT tokens and extracts Spotify tokens
func JWTMiddleware(jwtKey []byte, logger *logrus.Logger) gin.HandlerFunc {
	return func(c *gin.Context) {
		authHeader := c.GetHeader("Authorization")
		if authHeader == "" {
			logger.Warn("Missing Authorization header")
			c.JSON(401, gin.H{"error": "Authorization required"})
			c.Abort()
			return
		}

		// Extract token from "Bearer <token>"
		tokenString := strings.TrimPrefix(authHeader, "Bearer ")
		if tokenString == authHeader {
			// No "Bearer " prefix found
			logger.Warn("Invalid Authorization header format")
			c.JSON(401, gin.H{"error": "Invalid authorization format"})
			c.Abort()
			return
		}

		// Parse and validate token
		token, err := jwt.ParseWithClaims(tokenString, &model.Claims{}, func(token *jwt.Token) (interface{}, error) {
			// Validate signing method
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, jwt.ErrSignatureInvalid
			}
			return jwtKey, nil
		})

		if err != nil {
			logger.Warnf("Failed to parse JWT: %v", err)
			c.JSON(401, gin.H{"error": "Invalid token"})
			c.Abort()
			return
		}

		if !token.Valid {
			logger.Warn("Invalid JWT token")
			c.JSON(401, gin.H{"error": "Invalid token"})
			c.Abort()
			return
		}

		// Extract claims
		claims, ok := token.Claims.(*model.Claims)
		if !ok {
			logger.Warn("Failed to extract claims from JWT")
			c.JSON(401, gin.H{"error": "Invalid token claims"})
			c.Abort()
			return
		}

		// Store claims and Spotify token in context
		c.Set("claims", claims)
		c.Set("spotify_token", claims.AccessToken)
		c.Set("refresh_token", claims.RefreshToken)

		c.Next()
	}
}

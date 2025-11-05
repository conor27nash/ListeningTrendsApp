package logger

import (
	"os"

	"github.com/sirupsen/logrus"
)

// NewLogger creates a new configured logger instance
func NewLogger(serviceName string) *logrus.Logger {
	log := logrus.New()

	// Set output to stdout
	log.SetOutput(os.Stdout)

	// Set log format
	log.SetFormatter(&logrus.JSONFormatter{
		TimestampFormat: "2006-01-02T15:04:05.000Z07:00",
	})

	// Set log level from environment or default to Info
	level := os.Getenv("LOG_LEVEL")
	switch level {
	case "debug":
		log.SetLevel(logrus.DebugLevel)
	case "info":
		log.SetLevel(logrus.InfoLevel)
	case "warn":
		log.SetLevel(logrus.WarnLevel)
	case "error":
		log.SetLevel(logrus.ErrorLevel)
	default:
		log.SetLevel(logrus.InfoLevel)
	}

	// Add default fields
	log.WithField("service", serviceName)

	return log
}

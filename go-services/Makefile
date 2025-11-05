# Makefile for ListeningTrendsApp Go Services

.PHONY: help build build-all test test-coverage clean run-user run-all docker-build docker-up docker-down fmt vet lint

# Default target
help:
	@echo "Available targets:"
	@echo "  make build SERVICE=<name>  - Build specific service"
	@echo "  make build-all             - Build all services"
	@echo "  make test                  - Run all tests"
	@echo "  make test-coverage         - Run tests with coverage"
	@echo "  make clean                 - Remove build artifacts"
	@echo "  make run-user              - Run user service locally"
	@echo "  make run-all               - Run all services locally (requires tmux)"
	@echo "  make docker-build          - Build Docker images"
	@echo "  make docker-up             - Start all services with Docker Compose"
	@echo "  make docker-down           - Stop all Docker services"
	@echo "  make fmt                   - Format Go code"
	@echo "  make vet                   - Run go vet"
	@echo "  make lint                  - Run golangci-lint (if installed)"

# Build specific service
build:
	@if [ -z "$(SERVICE)" ]; then \
		echo "Error: SERVICE not specified. Usage: make build SERVICE=user"; \
		exit 1; \
	fi
	@echo "Building $(SERVICE) service..."
	@go build -o bin/$(SERVICE)-service ./cmd/$(SERVICE)
	@echo "Built bin/$(SERVICE)-service"

# Build all services
build-all:
	@echo "Building all services..."
	@mkdir -p bin
	@for service in user recentlyplayed topitems artist track analytics server; do \
		echo "Building $$service..."; \
		go build -o bin/$$service-service ./cmd/$$service || exit 1; \
	done
	@echo "All services built successfully"

# Run tests
test:
	@echo "Running tests..."
	@go test -v ./...

# Run tests with coverage
test-coverage:
	@echo "Running tests with coverage..."
	@go test -cover -coverprofile=coverage.out ./...
	@go tool cover -html=coverage.out -o coverage.html
	@echo "Coverage report: coverage.html"

# Clean build artifacts
clean:
	@echo "Cleaning build artifacts..."
	@rm -rf bin/
	@rm -f coverage.out coverage.html
	@echo "Clean complete"

# Run user service locally
run-user:
	@echo "Running user service on port 5005..."
	@PORT=5005 LOG_LEVEL=debug go run cmd/user/main.go

# Run recently played service
run-recentlyplayed:
	@echo "Running recently played service on port 5002..."
	@PORT=5002 LOG_LEVEL=debug go run cmd/recentlyplayed/main.go

# Run top items service
run-topitems:
	@echo "Running top items service on port 5001..."
	@PORT=5001 LOG_LEVEL=debug go run cmd/topitems/main.go

# Run artist service
run-artist:
	@echo "Running artist service on port 5004..."
	@PORT=5004 LOG_LEVEL=debug go run cmd/artist/main.go

# Run track service
run-track:
	@echo "Running track service on port 5003..."
	@PORT=5003 LOG_LEVEL=debug go run cmd/track/main.go

# Run analytics service
run-analytics:
	@echo "Running analytics service on port 5006..."
	@PORT=5006 TOPITEMS_SERVICE_URL=http://localhost:5001 LOG_LEVEL=debug go run cmd/analytics/main.go

# Run server/gateway
run-server:
	@echo "Running server/gateway on port 5000..."
	@PORT=5000 LOG_LEVEL=debug go run cmd/server/main.go

# Build Docker images for all services
docker-build:
	@echo "Building Docker images..."
	@docker-compose -f docker-compose.go.yml build

# Start all services with Docker Compose
docker-up:
	@echo "Starting services with Docker Compose..."
	@docker-compose -f docker-compose.go.yml up -d

# Stop all Docker services
docker-down:
	@echo "Stopping Docker services..."
	@docker-compose -f docker-compose.go.yml down

# View Docker logs
docker-logs:
	@docker-compose -f docker-compose.go.yml logs -f

# Format Go code
fmt:
	@echo "Formatting Go code..."
	@go fmt ./...

# Run go vet
vet:
	@echo "Running go vet..."
	@go vet ./...

# Run golangci-lint (if installed)
lint:
	@if command -v golangci-lint >/dev/null 2>&1; then \
		echo "Running golangci-lint..."; \
		golangci-lint run; \
	else \
		echo "golangci-lint not installed. Install with: go install github.com/golangci/golangci-lint/cmd/golangci-lint@latest"; \
	fi

# Download dependencies
deps:
	@echo "Downloading dependencies..."
	@go mod download
	@go mod verify

# Tidy dependencies
tidy:
	@echo "Tidying dependencies..."
	@go mod tidy

# Generate mocks (if using mockgen)
mocks:
	@echo "Generating mocks..."
	@go generate ./...

# Install development tools
install-tools:
	@echo "Installing development tools..."
	@go install github.com/golangci/golangci-lint/cmd/golangci-lint@latest
	@echo "Tools installed"

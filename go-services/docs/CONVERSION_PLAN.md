# Complete C# to Go Conversion Plan

## Overview

This document outlines the complete plan for converting the ListeningTrendsApp microservices from C#/.NET to Go.

## Project Status

- [x] Phase 0: Foundation Complete
  - [x] Project structure created
  - [x] Shared models defined
  - [x] Utility packages created
  - [x] Docker infrastructure set up
  - [x] Documentation written

- [ ] Phase 1: Simple Services (Week 1-2)
  - [ ] User Service
  - [ ] Recently Played Service
  - [ ] Top Items Service

- [ ] Phase 2: Medium Services (Week 3-4)
  - [ ] Artist Service
  - [ ] Track Service

- [ ] Phase 3: Complex Service (Week 5)
  - [ ] Analytics Service

- [ ] Phase 4: Gateway (Week 6-7)
  - [ ] Server/Gateway Service

- [ ] Phase 5: Testing & Deployment (Week 8)
  - [ ] Integration testing
  - [ ] Performance testing
  - [ ] Production deployment

---

## Phase 0: Foundation ✅ COMPLETE

### Completed

1. **Project Structure**
   - ✅ Created cmd/ directories for all services
   - ✅ Created internal/ directories for service logic
   - ✅ Created pkg/ for shared packages
   - ✅ Set up go.mod with dependencies

2. **Shared Models** (`pkg/models/`)
   - ✅ Artist model with response types
   - ✅ Track model with response types
   - ✅ Album model
   - ✅ User model
   - ✅ Image model
   - ✅ Analytics models (all 5 types)

3. **Utility Packages**
   - ✅ Logger (pkg/logger/) - Structured JSON logging
   - ✅ Config (pkg/config/) - Environment-based configuration
   - ✅ HTTP Client (pkg/httpclient/) - Reusable HTTP wrapper
   - ✅ Spotify Client (pkg/spotify/) - Spotify API wrapper

4. **Docker Infrastructure**
   - ✅ Dockerfile.template - Multi-stage build
   - ✅ docker-compose.go.yml - All services defined
   - ✅ .dockerignore
   - ✅ .env.example

5. **Documentation**
   - ✅ README.md - Project overview
   - ✅ IMPLEMENTATION_GUIDE.md - Step-by-step guide
   - ✅ CSHARP_TO_GO_REFERENCE.md - Syntax reference
   - ✅ This CONVERSION_PLAN.md

6. **Development Tools**
   - ✅ Makefile - Build and run commands
   - ✅ .gitignore

---

## Phase 1: Simple Services

### 1.1 User Service (Day 1-2) ⭐ START HERE

**Implementation Steps:**

1. **Create Handler** (`internal/user/handler/user_handler.go`)
   ```bash
   # File location
   go-services/internal/user/handler/user_handler.go
   ```
   - [ ] Define UserHandler struct
   - [ ] Implement GetProfile() - GET /api/user/profile
   - [ ] Implement GetFollowedArtists() - GET /api/user/following/artists
   - [ ] Add parameter validation (limit: 1-50)

2. **Create Main** (`cmd/user/main.go`)
   ```bash
   # File location
   go-services/cmd/user/main.go
   ```
   - [ ] Load config
   - [ ] Setup logger
   - [ ] Setup Spotify client
   - [ ] Setup router
   - [ ] Register routes
   - [ ] Add health check endpoint

3. **Test Locally**
   ```bash
   make run-user
   # Or
   PORT=5005 go run cmd/user/main.go

   # Test
   curl http://localhost:5005/health
   ```

4. **Write Tests** (`internal/user/handler/user_handler_test.go`)
   - [ ] Test missing authorization
   - [ ] Test invalid parameters
   - [ ] Test successful responses

5. **Build and Deploy**
   ```bash
   make build SERVICE=user
   docker build --build-arg SERVICE=user -f Dockerfile.template -t user-service .
   ```

**C# Reference Files:**
- UserService/Controllers/UserController.cs (lines 22-112)
- UserService/Program.cs

**Estimated Time:** 2-3 days

---

### 1.2 Recently Played Service (Day 3-4)

**Implementation Steps:**

1. **Create Handler** (`internal/recentlyplayed/handler/recentlyplayed_handler.go`)
   - [ ] Define RecentlyPlayedHandler struct
   - [ ] Implement GetRecentTracks() - GET /api/recentlyplayed/recent-tracks
   - [ ] Handle query parameters: limit, after, before
   - [ ] Validate limit (1-50)

2. **Create Main** (`cmd/recentlyplayed/main.go`)
   - [ ] Same pattern as User Service
   - [ ] Single route registration

3. **Test Locally**
   ```bash
   make run-recentlyplayed
   ```

4. **Write Tests**
   - [ ] Test parameter validation
   - [ ] Test query string building

5. **Build and Deploy**
   ```bash
   make build SERVICE=recentlyplayed
   ```

**C# Reference Files:**
- RecentlyPlayedService/Controllers/RecentlyPlayedController.cs (lines 19-52)

**Estimated Time:** 2 days

---

### 1.3 Top Items Service (Day 5-7)

**Implementation Steps:**

1. **Create Handlers**
   - `internal/topitems/handler/topartists_handler.go`
     - [ ] Implement GetTopArtists() - GET /api/topartists/top-artists/:timeRange

   - `internal/topitems/handler/toptracks_handler.go`
     - [ ] Implement GetTopTracks() - GET /api/toptracks/top-tracks/:timeRange

2. **Path Parameter Validation**
   - [ ] Validate timeRange in: ["short_term", "medium_term", "long_term"]

3. **Create Main** (`cmd/topitems/main.go`)
   - [ ] Register both handlers
   - [ ] Two separate routes

4. **Test Locally**
   ```bash
   make run-topitems
   ```

5. **Build and Deploy**
   ```bash
   make build SERVICE=topitems
   ```

**C# Reference Files:**
- TopItemsService/Controllers/TopArtistsController.cs (lines 19-41)
- TopItemsService/Controllers/TopTracksController.cs (lines 19-41)

**Estimated Time:** 2-3 days

---

## Phase 2: Medium Services

### 2.1 Artist Service (Week 3)

**Implementation Steps:**

1. **Create Handler** (`internal/artist/handler/artist_handler.go`)
   - [ ] GetArtist() - GET /api/artists/:id
   - [ ] GetSeveralArtists() - GET /api/artists/several?ids=...
   - [ ] GetArtistTopTracks() - GET /api/artists/:id/top-tracks
   - [ ] GetArtistAlbums() - GET /api/artists/:id/albums

2. **Important: Route Order**
   ```go
   // Register specific routes BEFORE parameterized routes
   router.GET("/api/artists/several", handler.GetSeveral)
   router.GET("/api/artists/:id", handler.GetArtist)
   router.GET("/api/artists/:id/top-tracks", handler.GetTopTracks)
   router.GET("/api/artists/:id/albums", handler.GetAlbums)
   ```

3. **CSV ID Handling**
   - [ ] Split comma-separated IDs
   - [ ] Validate max 50 IDs

4. **Query Parameters**
   - [ ] Handle optional parameters (market, include_groups, limit, offset)
   - [ ] Validate and clamp limit (1-50)

5. **Create Main** (`cmd/artist/main.go`)

6. **Test Locally**
   ```bash
   make run-artist
   ```

**C# Reference Files:**
- ArtistService/Controllers/ArtistsController.cs (lines 22-243)

**Estimated Time:** 4-5 days

---

### 2.2 Track Service (Week 4)

**Implementation Steps:**

1. **Create Handler** (`internal/track/handler/track_handler.go`)
   - [ ] GetTrack() - GET /api/tracks/:id
   - [ ] GetSeveralTracks() - GET /api/tracks/several?ids=...
   - [ ] GetSavedTracks() - GET /api/tracks/saved
   - [ ] SaveTracks() - PUT /api/tracks/save
   - [ ] RemoveSavedTracks() - DELETE /api/tracks/remove
   - [ ] CheckSavedTracks() - GET /api/tracks/check-saved?ids=...

2. **Request Body Structs**
   ```go
   type SaveTracksRequest struct {
       IDs []string `json:"ids" binding:"required"`
   }
   ```

3. **HTTP Methods**
   - [ ] GET endpoints
   - [ ] PUT with JSON body
   - [ ] DELETE with JSON body (requires body handling)

4. **Create Main** (`cmd/track/main.go`)
   - [ ] Register all 6 routes with correct HTTP methods

5. **Test Locally**
   ```bash
   make run-track
   ```

**C# Reference Files:**
- TrackService/Controllers/TracksController.cs (lines 22-364)

**Estimated Time:** 5-6 days

---

## Phase 3: Complex Service

### 3.1 Analytics Service (Week 5)

**Implementation Steps:**

1. **Create TopItems Client** (`internal/analytics/service/topitems_client.go`)
   - [ ] GetTopTracks() - Calls TopItems service
   - [ ] GetTopArtists() - Calls TopItems service
   - [ ] Handle both response formats: array vs {items: [...]}

2. **Create Analytics Service** (`internal/analytics/service/analytics_service.go`)
   - [ ] GenerateAnalytics() - Main orchestration method
   - [ ] generateAlbumMosaic() - Album frequency
   - [ ] generateTopArtist() - Most frequent artist
   - [ ] generateArtistLeaderboard() - Ranked artists
   - [ ] generateTrackTimeline() - Chronological tracks
   - [ ] generateGenreBubbleChart() - Genre frequency

3. **Algorithm Conversions (LINQ → Go)**

   **Album Mosaic:**
   ```go
   albumMap := make(map[string]*models.AlbumMosaic)
   for _, track := range tracks {
       if _, exists := albumMap[track.Album.Name]; !exists {
           albumMap[track.Album.Name] = &models.AlbumMosaic{...}
       } else {
           albumMap[track.Album.Name].Count++
       }
   }
   ```

   **Top Artist:**
   ```go
   artistCounts := make(map[string]*ArtistCount)
   // Count occurrences
   // Find max
   ```

   **Track Timeline:**
   ```go
   // Filter invalid dates
   // Parse dates (handle YYYY-MM-DD and YYYY formats)
   // Sort chronologically
   sort.Slice(timeline, func(i, j int) bool {
       return timeline[i].ReleaseDate.Before(timeline[j].ReleaseDate)
   })
   ```

   **Genre Bubble:**
   ```go
   genreCounts := make(map[string]int)
   for _, artist := range artists {
       for _, genre := range artist.Genres {
           genreCounts[genre]++
       }
   }
   // Sort by count descending
   ```

4. **Create Handler** (`internal/analytics/handler/analytics_handler.go`)
   - [ ] GetAnalytics() - GET /api/analytics/analytics?timeRange=...
   - [ ] RefreshAnalytics() - POST /api/analytics/refresh (stub)

5. **Create Main** (`cmd/analytics/main.go`)
   - [ ] Setup TopItems client (not Spotify client!)
   - [ ] Setup analytics service
   - [ ] Register routes

6. **Test Locally**
   ```bash
   # Must run TopItems service first!
   make run-topitems
   # Then
   make run-analytics
   ```

**C# Reference Files:**
- AnalyticsService/Services/AnalyticsService.cs (lines 16-189)
- AnalyticsService/Services/TopItemsService.cs (lines 20-281)
- AnalyticsService/Controllers/AnalyticsController.cs (lines 34-56)

**Estimated Time:** 7-8 days

---

## Phase 4: Gateway (Week 6-7)

### 4.1 Server/Gateway Service

**This is the most complex service - requires all others to be complete.**

**Implementation Steps:**

1. **Create JWT Models** (`internal/server/model/jwt.go`)
   ```go
   type Claims struct {
       AccessToken  string `json:"access_token"`
       RefreshToken string `json:"refresh_token"`
       jwt.RegisteredClaims
   }
   ```

2. **Create Token Service** (`internal/server/service/token_service.go`)
   - [ ] GetAccessToken() - Exchange code for tokens
   - [ ] RefreshAccessToken() - Refresh Spotify token
   - [ ] Call Spotify OAuth endpoints

3. **Create JWT Middleware** (`internal/server/middleware/jwt.go`)
   - [ ] Parse and validate JWT
   - [ ] Extract Spotify tokens from claims
   - [ ] Add to Gin context

4. **Create Auth Handler** (`internal/server/handler/auth_handler.go`)
   - [ ] Connect() - GET /api/login/connect (redirect to Spotify)
   - [ ] Callback() - GET /api/login/callback (exchange code, generate JWT)
   - [ ] RefreshToken() - POST /api/login/refresh (refresh JWT)

5. **Create Proxy Handler** (`internal/server/handler/proxy_handler.go`)
   - [ ] ProxyToService() - Generic proxy function
   - [ ] Extract Spotify token from JWT
   - [ ] Forward request to microservice
   - [ ] Return response

6. **Create Main** (`cmd/server/main.go`)
   - [ ] Setup all HTTP clients (7 services)
   - [ ] Setup JWT middleware
   - [ ] Setup CORS middleware
   - [ ] Register auth routes (no JWT)
   - [ ] Register protected routes (with JWT)
   - [ ] Register proxy routes to all services
   - [ ] Setup static file serving (React app)

7. **OAuth Flow Implementation**
   ```go
   // Build Spotify authorization URL
   params := url.Values{}
   params.Set("client_id", cfg.SpotifyClientID)
   params.Set("response_type", "code")
   params.Set("redirect_uri", cfg.RedirectURI)
   params.Set("scope", cfg.Scopes)
   params.Set("state", uuid.New().String())

   authURL := "https://accounts.spotify.com/authorize?" + params.Encode()
   ```

8. **JWT Generation**
   ```go
   claims := &Claims{
       AccessToken:  spotifyTokens.AccessToken,
       RefreshToken: spotifyTokens.RefreshToken,
       RegisteredClaims: jwt.RegisteredClaims{
           ExpiresAt: jwt.NewNumericDate(time.Now().Add(60 * time.Minute)),
           Issuer:    cfg.JWTIssuer,
           Audience:  jwt.ClaimStrings{cfg.JWTAudience},
       },
   }

   token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
   tokenString, _ := token.SignedString(jwtKey)
   ```

9. **Test Locally**
   ```bash
   # All services must be running!
   make run-server
   ```

10. **Integration Testing**
    - [ ] Test full OAuth flow
    - [ ] Test all proxy endpoints
    - [ ] Test JWT refresh
    - [ ] Test with actual frontend

**C# Reference Files:**
- SpotifyTrendsApp.Server/Program.cs (lines 11-144)
- SpotifyTrendsApp.Server/Controllers/LoginController.cs (lines 27-114)
- SpotifyTrendsApp.Server/Services/TokenService.cs
- SpotifyTrendsApp.Server/Controllers/*ProxyController.cs

**Estimated Time:** 10-12 days

---

## Phase 5: Testing & Deployment (Week 8)

### 5.1 Integration Testing

- [ ] Test all services together
- [ ] Test service-to-service communication
- [ ] Test error handling
- [ ] Test with real Spotify data

### 5.2 Performance Testing

- [ ] Benchmark against C# services
- [ ] Load testing
- [ ] Memory profiling
- [ ] Identify bottlenecks

### 5.3 Documentation

- [ ] API documentation
- [ ] Deployment guide
- [ ] Operations runbook
- [ ] Migration guide

### 5.4 Production Deployment

- [ ] Deploy to staging environment
- [ ] Run smoke tests
- [ ] Gradual rollout (Strangler Fig pattern)
- [ ] Monitor metrics
- [ ] Rollback plan ready

---

## Migration Strategy

### Option 1: Big Bang (Not Recommended)
- Convert all services at once
- Deploy all together
- High risk

### Option 2: Gradual Migration (Recommended)

**Week by Week:**
1. Deploy User Service Go → route 10% traffic → monitor → 100%
2. Deploy RecentlyPlayed Service Go → same process
3. Deploy TopItems Service Go → same process
4. Deploy Artist Service Go → same process
5. Deploy Track Service Go → same process
6. Deploy Analytics Service Go → depends on TopItems
7. Deploy Gateway Go → final switchover

### Option 3: Strangler Fig (Best for Production)
- Keep C# services running
- Add Go services alongside
- Gateway routes to whichever is available
- Gradually retire C# services

---

## Success Criteria

- [ ] All endpoints functional
- [ ] Performance meets or exceeds C# version
- [ ] Zero data loss
- [ ] Frontend works without changes
- [ ] Error rates < 0.1%
- [ ] Response times < C# baseline + 10%
- [ ] Memory usage < C# baseline

---

## Rollback Plan

If issues occur during migration:

1. **Service Level Rollback**
   - Switch traffic back to C# service
   - Keep Go service running for debugging

2. **Gateway Level Rollback**
   - Configure gateway to route to C# services
   - Fix Go issues offline

3. **Full Rollback**
   - docker-compose down go services
   - docker-compose up C# services
   - Update DNS/load balancer

---

## Team Responsibilities

### Developer 1: Backend Services
- User Service
- Recently Played Service
- Artist Service

### Developer 2: Backend Services
- Top Items Service
- Track Service
- Analytics Service

### Developer 3: Gateway & DevOps
- Server/Gateway
- Docker setup
- CI/CD pipeline
- Deployment

---

## Daily Checklist

For each service implementation:

- [ ] Read C# implementation
- [ ] Create Go handler
- [ ] Create main.go
- [ ] Test locally with real Spotify token
- [ ] Write unit tests
- [ ] Build Docker image
- [ ] Test in Docker
- [ ] Document any issues or deviations
- [ ] Update this checklist

---

## Risk Management

### High Risk Areas

1. **JWT Implementation**
   - Risk: Security vulnerabilities
   - Mitigation: Use well-tested libraries, code review

2. **OAuth Flow**
   - Risk: Token leakage, CSRF
   - Mitigation: Follow OAuth 2.0 best practices, use state parameter

3. **Service-to-Service Communication**
   - Risk: Network failures, timeouts
   - Mitigation: Implement retries, circuit breakers

4. **Data Parsing**
   - Risk: JSON parsing errors with unexpected formats
   - Mitigation: Defensive parsing, handle both formats

### Medium Risk Areas

1. **Date Parsing** (Track Timeline)
   - Multiple date formats from Spotify
   - Handle gracefully

2. **CSV ID Parsing**
   - Validate max limits
   - Handle malformed input

3. **Query Parameter Validation**
   - Sanitize all inputs
   - Clamp numeric values

---

## Next Steps

1. **Start with User Service** (Day 1)
   - Follow implementation guide
   - Test thoroughly
   - Build confidence

2. **Move to Simple Services** (Week 1-2)
   - Establish patterns
   - Create reusable code

3. **Tackle Medium Services** (Week 3-4)
   - More complex but similar patterns

4. **Implement Analytics** (Week 5)
   - Most complex business logic
   - Requires TopItems to be done

5. **Complete Gateway** (Week 6-7)
   - Brings it all together
   - Full integration

6. **Test and Deploy** (Week 8)
   - Confidence in production

---

## Resources

- [Implementation Guide](./IMPLEMENTATION_GUIDE.md)
- [C# to Go Reference](./CSHARP_TO_GO_REFERENCE.md)
- [README](../README.md)
- [Go Documentation](https://go.dev/doc/)
- [Gin Documentation](https://gin-gonic.com/docs/)

---

**Ready to start? Begin with User Service!**

```bash
cd go-services
make run-user
```

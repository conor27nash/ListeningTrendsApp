# C# to Go Conversion Reference

Quick reference guide for converting C#/.NET patterns to Go equivalents.

## Table of Contents

1. [Basic Syntax](#basic-syntax)
2. [HTTP Server Setup](#http-server-setup)
3. [Controllers → Handlers](#controllers--handlers)
4. [Dependency Injection](#dependency-injection)
5. [Logging](#logging)
6. [Configuration](#configuration)
7. [JSON Handling](#json-handling)
8. [HTTP Requests](#http-requests)
9. [Error Handling](#error-handling)
10. [LINQ → Go Equivalents](#linq--go-equivalents)

---

## Basic Syntax

| C# | Go | Notes |
|---|---|---|
| `string name` | `name string` | Type comes after variable |
| `int count` | `count int` | |
| `List<T>` | `[]T` | Slice in Go |
| `Dictionary<K, V>` | `map[K]V` | |
| `string?` | `*string` | Nullable via pointer |
| `var x = 5` | `x := 5` | Type inference |
| `const string X = "hi"` | `const X = "hi"` | |
| `if (condition) { }` | `if condition { }` | No parentheses |
| `foreach (var item in list)` | `for _, item := range list` | |
| `Task<T>` | No direct equivalent | Go uses goroutines |
| `async/await` | `go func()` + channels | Different concurrency model |

---

## HTTP Server Setup

### C# (ASP.NET Core)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
```

### Go (Gin)

```go
// main.go
package main

import "github.com/gin-gonic/gin"

func main() {
    router := gin.Default()
    router.GET("/endpoint", handler)
    router.Run(":5000")
}
```

---

## Controllers → Handlers

### C# Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserController> _logger;

    public UserController(HttpClient httpClient, ILogger<UserController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromHeader] string Authorization)
    {
        if (string.IsNullOrEmpty(Authorization))
        {
            return Unauthorized("Bearer token is required");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        request.Headers.Add("Authorization", Authorization);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var json = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<object>(json);
        return Ok(profile);
    }
}
```

### Go Handler

```go
package handler

import (
    "encoding/json"
    "github.com/gin-gonic/gin"
    "github.com/sirupsen/logrus"
)

type UserHandler struct {
    spotifyClient *spotify.Client
    logger        *logrus.Logger
}

func NewUserHandler(spotifyClient *spotify.Client, logger *logrus.Logger) *UserHandler {
    return &UserHandler{
        spotifyClient: spotifyClient,
        logger:        logger,
    }
}

func (h *UserHandler) GetProfile(c *gin.Context) {
    auth := c.GetHeader("Authorization")
    if auth == "" {
        c.JSON(401, gin.H{"error": "Bearer token is required"})
        return
    }

    data, statusCode, err := h.spotifyClient.Get("/me", auth)
    if err != nil {
        h.logger.Errorf("Error: %v", err)
        c.JSON(500, gin.H{"error": "Internal server error"})
        return
    }

    if statusCode != 200 {
        c.Data(statusCode, "application/json", data)
        return
    }

    var profile interface{}
    if err := json.Unmarshal(data, &profile); err != nil {
        c.JSON(500, gin.H{"error": "Failed to parse response"})
        return
    }

    c.JSON(200, profile)
}
```

**Key Differences:**
- No attributes/decorators in Go
- Routes defined explicitly in main.go
- No async/await (Go uses goroutines)
- Error handling is explicit (no exceptions)
- Gin context replaces IActionResult

---

## Dependency Injection

### C# (Built-in DI)

```csharp
// Program.cs
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddHttpClient("Spotify");

// Controller
public UserController(IHttpClientFactory factory, ITokenService tokenService)
{
    _httpClient = factory.CreateClient("Spotify");
    _tokenService = tokenService;
}
```

### Go (Manual Construction)

```go
// main.go
cfg := config.LoadConfig()
logger := logger.NewLogger("service")
spotifyClient := spotify.NewClient(cfg.SpotifyBaseURL)
handler := handler.NewHandler(spotifyClient, logger)

// No DI container - dependencies passed explicitly
```

**Key Differences:**
- Go doesn't have built-in DI
- Dependencies are passed via constructors (New functions)
- Simpler but more manual

---

## Logging

### C# (ILogger)

```csharp
_logger.LogInformation("Fetching user profile");
_logger.LogWarning("Missing auth header");
_logger.LogError(ex, "Error fetching profile");
```

### Go (logrus)

```go
log.Info("Fetching user profile")
log.Warn("Missing auth header")
log.Errorf("Error fetching profile: %v", err)
```

**Setting Up Logger:**

```go
// pkg/logger/logger.go
import "github.com/sirupsen/logrus"

func NewLogger(serviceName string) *logrus.Logger {
    log := logrus.New()
    log.SetFormatter(&logrus.JSONFormatter{})
    log.SetLevel(logrus.InfoLevel)
    return log
}
```

---

## Configuration

### C# (appsettings.json + Environment Variables)

```csharp
// appsettings.json
{
  "Jwt": {
    "Key": "secret",
    "Issuer": "MyApp"
  }
}

// Usage
var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key");
```

### Go (Environment Variables)

```go
// pkg/config/config.go
type Config struct {
    JWTKey    string
    JWTIssuer string
}

func LoadConfig() *Config {
    return &Config{
        JWTKey:    os.Getenv("JWT_KEY"),
        JWTIssuer: getEnv("JWT_ISSUER", "MyApp"),
    }
}

func getEnv(key, defaultValue string) string {
    if value := os.Getenv(key); value != "" {
        return value
    }
    return defaultValue
}
```

---

## JSON Handling

### C# Models

```csharp
public class Artist
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("genres")]
    public string[] Genres { get; set; }
}

// Serialization
var json = JsonSerializer.Serialize(artist);
var artist = JsonSerializer.Deserialize<Artist>(json);
```

### Go Models

```go
type Artist struct {
    ID     string   `json:"id"`
    Name   string   `json:"name"`
    Genres []string `json:"genres,omitempty"` // omitempty = skip if nil/empty
}

// Serialization
json, _ := json.Marshal(artist)
var artist Artist
json.Unmarshal(json, &artist)
```

**Key Differences:**
- Go uses struct tags instead of attributes
- `omitempty` = don't include if empty/nil
- Exported fields must start with capital letter
- JSON keys are usually lowercase

---

## HTTP Requests

### C# (HttpClient)

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Add("Authorization", token);

var response = await _httpClient.SendAsync(request);
var content = await response.Content.ReadAsStringAsync();

if (!response.IsSuccessStatusCode)
{
    return StatusCode((int)response.StatusCode, content);
}
```

### Go (net/http)

```go
req, _ := http.NewRequest("GET", url, nil)
req.Header.Set("Authorization", token)

resp, err := httpClient.Do(req)
if err != nil {
    return nil, err
}
defer resp.Body.Close()

body, _ := io.ReadAll(resp.Body)

if resp.StatusCode != 200 {
    return nil, fmt.Errorf("status: %d", resp.StatusCode)
}
```

**Key Differences:**
- Must explicitly close body: `defer resp.Body.Close()`
- No async/await
- Error handling is explicit
- Status codes are ints, not enums

---

## Error Handling

### C# (Exceptions)

```csharp
try
{
    var result = await DoSomething();
    return Ok(result);
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Request failed");
    return StatusCode(500, "Internal server error");
}
```

### Go (Error Values)

```go
result, err := DoSomething()
if err != nil {
    log.Errorf("Request failed: %v", err)
    c.JSON(500, gin.H{"error": "Internal server error"})
    return
}

c.JSON(200, result)
```

**Key Differences:**
- Go returns errors as values, not exceptions
- Must explicitly check `if err != nil`
- More verbose but explicit

---

## LINQ → Go Equivalents

### C# LINQ

```csharp
// GroupBy
var grouped = tracks
    .GroupBy(t => t.Album.Name)
    .Select(g => new {
        AlbumName = g.Key,
        Count = g.Count()
    })
    .ToList();

// Where
var filtered = tracks
    .Where(t => t.Popularity > 50)
    .ToList();

// Select
var names = artists
    .Select(a => a.Name)
    .ToList();

// OrderBy
var sorted = items
    .OrderByDescending(i => i.Score)
    .ToList();

// First/FirstOrDefault
var first = items.FirstOrDefault();

// Any
bool hasItems = items.Any();

// Sum/Average
var total = items.Sum(i => i.Value);
var avg = items.Average(i => i.Value);
```

### Go Equivalents

```go
// GroupBy - use map
grouped := make(map[string]int)
for _, track := range tracks {
    grouped[track.Album.Name]++
}

// Where - use loop + append
filtered := []Track{}
for _, track := range tracks {
    if track.Popularity > 50 {
        filtered = append(filtered, track)
    }
}

// Select - use loop
names := make([]string, len(artists))
for i, artist := range artists {
    names[i] = artist.Name
}

// OrderBy - use sort package
sort.Slice(items, func(i, j int) bool {
    return items[i].Score > items[j].Score // Descending
})

// First/FirstOrDefault
var first *Item
if len(items) > 0 {
    first = &items[0]
}

// Any
hasItems := len(items) > 0

// Sum/Average
total := 0
for _, item := range items {
    total += item.Value
}
avg := float64(total) / float64(len(items))
```

**Key Differences:**
- No LINQ in Go - use loops and standard library
- More verbose but explicit
- Use `sort` package for sorting
- Use maps for grouping

---

## Common C# → Go Mappings

| C# Feature | Go Equivalent |
|-----------|---------------|
| `async/await` | goroutines + channels |
| `Task<T>` | function returning `(T, error)` |
| `null` | `nil` (for pointers/interfaces) |
| `?.` (null conditional) | `if x != nil { x.Field }` |
| `??` (null coalescing) | Custom function or ternary-like logic |
| `Tuple<A, B>` | Return multiple values: `(A, B)` |
| `var` | `:=` |
| `const` | `const` |
| `IEnumerable<T>` | `[]T` (slice) |
| `Dictionary<K, V>` | `map[K]V` |
| `HashSet<T>` | `map[T]bool` or `map[T]struct{}` |
| `List<T>.Add()` | `append(slice, item)` |
| `string.IsNullOrEmpty()` | `s == ""` |
| `int.Parse()` | `strconv.Atoi()` |
| `ToString()` | `fmt.Sprintf("%v", x)` |
| Properties | Fields (direct access) |
| `this` | Receiver (e.g., `func (h *Handler)`) |
| Interfaces | Implicit (no `implements` keyword) |
| Generics | Available in Go 1.18+ |

---

## Common Pitfalls

### 1. Forgotten Error Checks

```go
// ❌ Bad
data, _ := doSomething()  // Ignoring error

// ✅ Good
data, err := doSomething()
if err != nil {
    return err
}
```

### 2. Not Closing Response Bodies

```go
// ❌ Bad
resp, _ := http.Get(url)
body, _ := io.ReadAll(resp.Body)

// ✅ Good
resp, err := http.Get(url)
if err != nil {
    return err
}
defer resp.Body.Close()
body, _ := io.ReadAll(resp.Body)
```

### 3. Slice Append Confusion

```go
// ❌ Bad - append doesn't modify in place
items := []int{1, 2, 3}
append(items, 4)  // Returns new slice, doesn't modify items

// ✅ Good
items = append(items, 4)
```

### 4. Range Loop Variable Capture

```go
// ❌ Bad - all closures reference same variable
for _, item := range items {
    go func() {
        process(item)  // All goroutines see last item
    }()
}

// ✅ Good
for _, item := range items {
    item := item  // Create new variable
    go func() {
        process(item)
    }()
}
```

---

## Quick Reference: HTTP Status Codes

```go
// C#
return Ok(data);           // 200
return Created(uri, data); // 201
return NoContent();        // 204
return BadRequest(msg);    // 400
return Unauthorized(msg);  // 401
return NotFound(msg);      // 404
return StatusCode(500);    // 500

// Go (Gin)
c.JSON(200, data)
c.JSON(201, data)
c.Status(204)
c.JSON(400, gin.H{"error": msg})
c.JSON(401, gin.H{"error": msg})
c.JSON(404, gin.H{"error": msg})
c.JSON(500, gin.H{"error": msg})
```

---

## Resources

- [Go Tour](https://go.dev/tour/)
- [Effective Go](https://go.dev/doc/effective_go)
- [Gin Documentation](https://gin-gonic.com/docs/)
- [Go by Example](https://gobyexample.com/)

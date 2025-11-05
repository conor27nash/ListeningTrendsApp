package models

// Track represents a Spotify track
type Track struct {
	ID               string   `json:"id"`
	Name             string   `json:"name"`
	Album            Album    `json:"album,omitempty"`
	Artists          []Artist `json:"artists,omitempty"`
	AvailableMarkets []string `json:"available_markets,omitempty"`
	DiscNumber       int      `json:"disc_number,omitempty"`
	DurationMs       int      `json:"duration_ms,omitempty"`
	Explicit         bool     `json:"explicit,omitempty"`
	Href             string   `json:"href,omitempty"`
	IsPlayable       bool     `json:"is_playable,omitempty"`
	Popularity       int      `json:"popularity,omitempty"`
	PreviewURL       *string  `json:"preview_url,omitempty"`
	TrackNumber      int      `json:"track_number,omitempty"`
	Type             string   `json:"type,omitempty"`
	URI              string   `json:"uri,omitempty"`
	IsLocal          bool     `json:"is_local,omitempty"`
}

// TrackResponse represents the paginated track response from Spotify
type TrackResponse struct {
	Href   string  `json:"href"`
	Limit  int     `json:"limit"`
	Items  []Track `json:"items"`
	Offset int     `json:"offset"`
	Total  int     `json:"total"`
	Next   *string `json:"next"`
	Prev   *string `json:"previous"`
}

package models

// Album represents a Spotify album
type Album struct {
	ID               string   `json:"id"`
	Name             string   `json:"name"`
	Artists          []string `json:"artists,omitempty"` // Artist names as strings for simplified structure
	ReleaseDate      string   `json:"release_date,omitempty"`
	AlbumType        string   `json:"album_type,omitempty"`
	TotalTracks      int      `json:"total_tracks,omitempty"`
	Images           []Image  `json:"images,omitempty"`
	URI              string   `json:"uri,omitempty"`
	Href             string   `json:"href,omitempty"`
	AvailableMarkets []string `json:"available_markets,omitempty"`
	Type             string   `json:"type,omitempty"`
}

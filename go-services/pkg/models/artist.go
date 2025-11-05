package models

// Artist represents a Spotify artist
type Artist struct {
	ID               string   `json:"id"`
	Name             string   `json:"name"`
	Genres           []string `json:"genres,omitempty"`
	Followers        int      `json:"followers,omitempty"`
	ImageURL         string   `json:"image_url,omitempty"`
	AvailableMarkets []string `json:"available_markets,omitempty"`
	Href             string   `json:"href,omitempty"`
	Type             string   `json:"type,omitempty"`
	URI              string   `json:"uri,omitempty"`
	Images           []Image  `json:"images,omitempty"`
	Popularity       int      `json:"popularity,omitempty"`
}

// ArtistResponse represents the paginated artist response from Spotify
type ArtistResponse struct {
	Href   string   `json:"href"`
	Limit  int      `json:"limit"`
	Items  []Artist `json:"items"`
	Offset int      `json:"offset"`
	Total  int      `json:"total"`
	Next   *string  `json:"next"`
	Prev   *string  `json:"previous"`
}

// Followers represents artist/user follower count
type Followers struct {
	Href  *string `json:"href"`
	Total int     `json:"total"`
}

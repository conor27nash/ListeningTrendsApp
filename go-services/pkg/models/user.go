package models

// User represents a Spotify user profile
type User struct {
	ID          string     `json:"id"`
	DisplayName string     `json:"display_name,omitempty"`
	Email       string     `json:"email,omitempty"`
	Country     string     `json:"country,omitempty"`
	Images      []Image    `json:"images,omitempty"`
	Followers   *Followers `json:"followers,omitempty"`
	Href        string     `json:"href,omitempty"`
	Type        string     `json:"type,omitempty"`
	URI         string     `json:"uri,omitempty"`
	Product     string     `json:"product,omitempty"` // premium, free, etc.
}

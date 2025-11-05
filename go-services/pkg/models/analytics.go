package models

import "time"

// AnalyticsData represents the complete analytics response
type AnalyticsData struct {
	AlbumMosaicData        []AlbumMosaic       `json:"albumMosaicData"`
	TopArtistData          TopArtist           `json:"topArtistData"`
	ArtistLeaderboardData  []ArtistLeaderboard `json:"artistLeaderboardData"`
	TrackTimelineData      []TrackTimeline     `json:"trackTimelineData"`
	GenreBubbleData        []GenreBubble       `json:"genreBubbleData"`
}

// AlbumMosaic represents album frequency data
type AlbumMosaic struct {
	AlbumName   string `json:"albumName"`
	ArtistName  string `json:"artistName"`
	SpotifyLink string `json:"spotifyLink"`
	Count       int    `json:"count"`
}

// TopArtist represents the most frequent artist
type TopArtist struct {
	ArtistName  string  `json:"artistName"`
	SpotifyLink string  `json:"spotifyLink"`
	Count       int     `json:"count"`
	Tracks      []Track `json:"tracks"`
}

// ArtistLeaderboard represents ranked artist data
type ArtistLeaderboard struct {
	ArtistName    string `json:"artistName"`
	SpotifyLink   string `json:"spotifyLink"`
	Rank          int    `json:"rank"`
	FollowerCount int    `json:"followerCount"`
	Popularity    int    `json:"popularity"`
}

// TrackTimeline represents track chronological data
type TrackTimeline struct {
	TrackName   string    `json:"trackName"`
	ArtistName  string    `json:"artistName"`
	ReleaseDate time.Time `json:"releaseDate"`
	SpotifyLink string    `json:"spotifyLink"`
}

// GenreBubble represents genre frequency data
type GenreBubble struct {
	Genre string `json:"genre"`
	Count int    `json:"count"`
}

// MusicDNA represents user's music characteristics (future feature)
type MusicDNA struct {
	TopGenres         []string `json:"topGenres"`
	AveragePopularity float64  `json:"averagePopularity"`
	TotalTracks       int      `json:"totalTracks"`
	TotalArtists      int      `json:"totalArtists"`
}

// FollowVsTop represents followed vs top artists comparison (future feature)
type FollowVsTop struct {
	// TODO: Define structure based on requirements
}

package service

import (
	"sort"
	"strings"
	"time"

	"github.com/sirupsen/logrus"

	"github.com/conor27nash/ListeningTrendsApp/go-services/pkg/models"
)

// AnalyticsService generates analytics from user's top items
type AnalyticsService struct {
	topItemsClient *TopItemsClient
	logger         *logrus.Logger
}

// NewAnalyticsService creates a new AnalyticsService instance
func NewAnalyticsService(topItemsClient *TopItemsClient, logger *logrus.Logger) *AnalyticsService {
	return &AnalyticsService{
		topItemsClient: topItemsClient,
		logger:         logger,
	}
}

// GenerateAnalytics generates all analytics for a given time range
func (s *AnalyticsService) GenerateAnalytics(timeRange, auth string) (*models.AnalyticsData, error) {
	// Fetch data from TopItems service
	tracks, err := s.topItemsClient.GetTopTracks(timeRange, auth)
	if err != nil {
		return nil, err
	}

	artists, err := s.topItemsClient.GetTopArtists(timeRange, auth)
	if err != nil {
		return nil, err
	}

	s.logger.Infof("Generating analytics for %d tracks and %d artists", len(tracks), len(artists))

	// Generate all analytics
	return &models.AnalyticsData{
		AlbumMosaicData:       s.generateAlbumMosaic(tracks),
		TopArtistData:         s.generateTopArtist(tracks),
		ArtistLeaderboardData: s.generateArtistLeaderboard(artists),
		TrackTimelineData:     s.generateTrackTimeline(tracks),
		GenreBubbleData:       s.generateGenreBubbleChart(artists),
	}, nil
}

// generateAlbumMosaic creates album frequency data from tracks
func (s *AnalyticsService) generateAlbumMosaic(tracks []models.Track) []models.AlbumMosaic {
	albumMap := make(map[string]*models.AlbumMosaic)

	for _, track := range tracks {
		albumName := track.Album.Name
		if albumName == "" {
			continue
		}

		if _, exists := albumMap[albumName]; !exists {
			// Create artist names string
			artistNames := make([]string, len(track.Artists))
			for i, artist := range track.Artists {
				artistNames[i] = artist.Name
			}

			albumMap[albumName] = &models.AlbumMosaic{
				AlbumName:   albumName,
				ArtistName:  strings.Join(artistNames, ", "),
				SpotifyLink: track.Album.URI,
				Count:       1,
			}
		} else {
			albumMap[albumName].Count++
		}
	}

	// Convert map to slice
	result := make([]models.AlbumMosaic, 0, len(albumMap))
	for _, mosaic := range albumMap {
		result = append(result, *mosaic)
	}

	return result
}

// generateTopArtist finds the most frequent artist in top tracks
func (s *AnalyticsService) generateTopArtist(tracks []models.Track) models.TopArtist {
	if len(tracks) == 0 {
		return models.TopArtist{
			ArtistName: "No Artist Found",
			Count:      0,
			Tracks:     []models.Track{},
		}
	}

	// Count artist occurrences
	type artistCount struct {
		count  int
		uri    string
		tracks []models.Track
	}

	artistCounts := make(map[string]*artistCount)

	for _, track := range tracks {
		for _, artist := range track.Artists {
			if artist.Name == "" {
				continue
			}

			if _, exists := artistCounts[artist.Name]; !exists {
				artistCounts[artist.Name] = &artistCount{
					count:  1,
					uri:    artist.URI,
					tracks: []models.Track{track},
				}
			} else {
				artistCounts[artist.Name].count++
				artistCounts[artist.Name].tracks = append(artistCounts[artist.Name].tracks, track)
			}
		}
	}

	// Find the artist with the most tracks
	var topArtistName string
	var topCount int
	var topURI string
	var topTracks []models.Track

	for name, data := range artistCounts {
		if data.count > topCount {
			topCount = data.count
			topArtistName = name
			topURI = data.uri
			topTracks = data.tracks
		}
	}

	if topArtistName == "" {
		topArtistName = "No Artist Found"
	}

	return models.TopArtist{
		ArtistName:  topArtistName,
		SpotifyLink: topURI,
		Count:       topCount,
		Tracks:      topTracks,
	}
}

// generateArtistLeaderboard creates ranked artist data
func (s *AnalyticsService) generateArtistLeaderboard(artists []models.Artist) []models.ArtistLeaderboard {
	leaderboard := make([]models.ArtistLeaderboard, len(artists))

	for i, artist := range artists {
		leaderboard[i] = models.ArtistLeaderboard{
			ArtistName:    artist.Name,
			SpotifyLink:   artist.URI,
			Rank:          100 - (i * 2), // Custom ranking: 100, 98, 96, ...
			FollowerCount: artist.Followers,
			Popularity:    artist.Popularity,
		}
	}

	// Sort by rank descending
	sort.Slice(leaderboard, func(i, j int) bool {
		return leaderboard[i].Rank > leaderboard[j].Rank
	})

	return leaderboard
}

// generateTrackTimeline creates chronological track data
func (s *AnalyticsService) generateTrackTimeline(tracks []models.Track) []models.TrackTimeline {
	timeline := []models.TrackTimeline{}

	for _, track := range tracks {
		releaseDate := track.Album.ReleaseDate

		// Validate release date
		if releaseDate == "" || releaseDate == "0" || releaseDate == "0000" {
			continue
		}

		// Try parsing date (Spotify uses YYYY-MM-DD or YYYY)
		var parsedDate time.Time
		var err error

		// Try full date format first
		parsedDate, err = time.Parse("2006-01-02", releaseDate)
		if err != nil {
			// Try year-only format
			parsedDate, err = time.Parse("2006", releaseDate)
			if err != nil {
				// Skip if we can't parse
				continue
			}
		}

		// Filter out old/invalid dates
		if parsedDate.Year() <= 1900 {
			continue
		}

		// Create artist names string
		artistNames := make([]string, len(track.Artists))
		for i, artist := range track.Artists {
			artistNames[i] = artist.Name
		}

		timeline = append(timeline, models.TrackTimeline{
			TrackName:   track.Name,
			ArtistName:  strings.Join(artistNames, ", "),
			ReleaseDate: parsedDate,
			SpotifyLink: track.URI,
		})
	}

	// Sort by date ascending (oldest first)
	sort.Slice(timeline, func(i, j int) bool {
		return timeline[i].ReleaseDate.Before(timeline[j].ReleaseDate)
	})

	return timeline
}

// generateGenreBubbleChart creates genre frequency data
func (s *AnalyticsService) generateGenreBubbleChart(artists []models.Artist) []models.GenreBubble {
	genreCounts := make(map[string]int)

	for _, artist := range artists {
		for _, genre := range artist.Genres {
			if genre != "" {
				genreCounts[genre]++
			}
		}
	}

	// Convert to slice
	bubbles := make([]models.GenreBubble, 0, len(genreCounts))
	for genre, count := range genreCounts {
		bubbles = append(bubbles, models.GenreBubble{
			Genre: genre,
			Count: count,
		})
	}

	// Sort by count descending
	sort.Slice(bubbles, func(i, j int) bool {
		return bubbles[i].Count > bubbles[j].Count
	})

	return bubbles
}

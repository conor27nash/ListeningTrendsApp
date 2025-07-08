# API Documentation

This folder contains the organized API functions for the Spotify Trends app. The API is structured into separate modules for each service.

## File Structure

```
src/api/
├── index.js           # Main export file - import everything from here
├── base.js            # Base utilities (fetchWithAuth, refreshToken, etc.)
├── auth.js            # Authentication functions
├── topItems.js        # Top tracks and artists
├── recentlyPlayed.js  # Recently played tracks
├── tracks.js          # Track-related functions (get, save, remove, check)
├── artists.js         # Artist-related functions
└── user.js            # User profile and followed artists
```

## Usage

### Recommended Import Pattern

```javascript
// Import specific functions you need
import { getTopTracks, getTopArtists } from '../api/topItems';
import { getUserProfile } from '../api/user';
import { saveTracks, checkSavedTracks } from '../api/tracks';
```

### Alternative Import Pattern

```javascript
// Import everything from the main index (less preferred for performance)
import { getTopTracks, getUserProfile, saveTracks } from '../api';
```

## Available Functions

### Authentication (`auth.js`)
- `login()` - Redirect to Spotify login
- `logout()` - Clear token and redirect home
- `getAuthStatus()` - Check if user is authenticated

### Top Items (`topItems.js`)
- `getTopTracks(timeSpan)` - Get user's top tracks
- `getTopArtists(timeSpan)` - Get user's top artists

### Recently Played (`recentlyPlayed.js`)
- `getRecentlyPlayed(limit, before, after)` - Get recently played tracks

### Tracks (`tracks.js`)
- `getTrack(id, market)` - Get a single track
- `getSeveralTracks(ids, market)` - Get multiple tracks
- `getSavedTracks(limit, offset, market)` - Get user's saved tracks
- `saveTracks(trackIds)` - Save tracks to library
- `removeSavedTracks(trackIds)` - Remove tracks from library
- `checkSavedTracks(trackIds)` - Check if tracks are saved

### Artists (`artists.js`)
- `getArtist(id)` - Get a single artist
- `getSeveralArtists(ids)` - Get multiple artists
- `getArtistTopTracks(id, market)` - Get artist's top tracks
- `getArtistAlbums(id, includeGroups, market, limit, offset)` - Get artist's albums

### User (`user.js`)
- `getUserProfile()` - Get current user's profile
- `getFollowedArtists(after, limit)` - Get followed artists

## Authentication

All API functions automatically handle:
- JWT token management
- Automatic token refresh on 401 errors
- Authorization headers

The `fetchWithAuth` function from `base.js` handles all authentication logic transparently.

## Error Handling

All functions throw errors with descriptive messages that can be caught and handled in components:

```javascript
try {
  const tracks = await getTopTracks('short_term');
  // handle success
} catch (error) {
  console.error('Error fetching tracks:', error.message);
  // handle error
}
```

## Legacy Compatibility

The main `index.js` file includes legacy exports for backward compatibility:
- `TopItemsService_getTopTracks` → `getTopTracks`
- `TopItemsService_getTopArtists` → `getTopArtists`
- `RecentlyPlayedService_getRecentlyPlayed` → `getRecentlyPlayed`

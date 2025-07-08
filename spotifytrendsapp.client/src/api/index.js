/**
 * Central API exports - import all service APIs and re-export for convenience
 */

// Base utilities
export { BASE_URL, fetchWithAuth, refreshToken } from './base.js';

// Authentication
export * from './auth.js';

// Top Items (tracks and artists)
export * from './topItems.js';

// Recently Played
export * from './recentlyPlayed.js';

// Tracks
export * from './tracks.js';

// Artists
export * from './artists.js';

// User
export * from './user.js';

// Legacy compatibility exports (for existing code that might use these names)
export { getTopTracks as TopItemsService_getTopTracks } from './topItems.js';
export { getTopArtists as TopItemsService_getTopArtists } from './topItems.js';
export { getRecentlyPlayed as RecentlyPlayedService_getRecentlyPlayed } from './recentlyPlayed.js';

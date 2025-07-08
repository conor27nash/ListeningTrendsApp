import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * Top Items API functions
 */

export const getTopTracks = async (timeSpan) => {
  const url = `${BASE_URL}/api/toptracks/${timeSpan}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching top tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const getTopArtists = async (timeSpan) => {
  const url = `${BASE_URL}/api/topartists/${timeSpan}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching top artists: ${response.statusText}`);
  }
  return await response.json();
};

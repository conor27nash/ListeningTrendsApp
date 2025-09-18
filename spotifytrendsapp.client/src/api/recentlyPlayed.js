import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * Recently Played API functions
 */

export const getRecentlyPlayed = async (limit = 50, before = null, after = null) => {
  const queryParams = new URLSearchParams();
  queryParams.append('limit', limit.toString());
  if (before) queryParams.append('before', before);
  if (after) queryParams.append('after', after);
  
  const url = `${BASE_URL}/api/recentlyplayed/recent-tracks?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching recently played: ${response.statusText}`);
  }
  return await response.json();
};

import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * User API functions
 */

export const getUserProfile = async () => {
  const url = `${BASE_URL}/api/userproxy/profile`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching user profile: ${response.statusText}`);
  }
  return await response.json();
};

export const getFollowedArtists = async (after = null, limit = 20) => {
  const queryParams = new URLSearchParams();
  queryParams.append('limit', limit.toString());
  if (after) queryParams.append('after', after);
  
  const url = `${BASE_URL}/api/userproxy/following/artists?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching followed artists: ${response.statusText}`);
  }
  return await response.json();
};

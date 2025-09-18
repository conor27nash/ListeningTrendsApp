import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * Artists API functions
 */

export const getArtist = async (id) => {
  const url = `${BASE_URL}/api/artistsproxy/${id}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching artist: ${response.statusText}`);
  }
  return await response.json();
};

export const getSeveralArtists = async (ids) => {
  const idsParam = Array.isArray(ids) ? ids.join(',') : ids;
  const url = `${BASE_URL}/api/artistsproxy/several?ids=${idsParam}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching several artists: ${response.statusText}`);
  }
  return await response.json();
};

export const getArtistTopTracks = async (id, market = 'US') => {
  const url = `${BASE_URL}/api/artistsproxy/${id}/top-tracks?market=${market}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching artist top tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const getArtistAlbums = async (id, includeGroups = null, market = null, limit = 20, offset = 0) => {
  const queryParams = new URLSearchParams();
  queryParams.append('limit', limit.toString());
  queryParams.append('offset', offset.toString());
  if (includeGroups) queryParams.append('include_groups', includeGroups);
  if (market) queryParams.append('market', market);
  
  const url = `${BASE_URL}/api/artistsproxy/${id}/albums?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching artist albums: ${response.statusText}`);
  }
  return await response.json();
};

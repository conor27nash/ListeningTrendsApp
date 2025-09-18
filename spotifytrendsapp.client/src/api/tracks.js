import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * Tracks API functions
 */

export const getTrack = async (id, market = null) => {
  const queryParams = new URLSearchParams();
  if (market) queryParams.append('market', market);
  
  const url = `${BASE_URL}/api/tracksproxy/${id}?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching track: ${response.statusText}`);
  }
  return await response.json();
};

export const getSeveralTracks = async (ids, market = null) => {
  const queryParams = new URLSearchParams();
  const idsParam = Array.isArray(ids) ? ids.join(',') : ids;
  queryParams.append('ids', idsParam);
  if (market) queryParams.append('market', market);
  
  const url = `${BASE_URL}/api/tracksproxy/several?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching several tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const getSavedTracks = async (limit = 20, offset = 0, market = null) => {
  const queryParams = new URLSearchParams();
  queryParams.append('limit', limit.toString());
  queryParams.append('offset', offset.toString());
  if (market) queryParams.append('market', market);
  
  const url = `${BASE_URL}/api/tracksproxy/saved?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching saved tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const saveTracks = async (trackIds) => {
  const url = `${BASE_URL}/api/tracksproxy/save`;
  const response = await fetchWithAuth(url, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ids: Array.isArray(trackIds) ? trackIds : [trackIds] })
  });
  if (!response.ok) {
    throw new Error(`Error saving tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const removeSavedTracks = async (trackIds) => {
  const url = `${BASE_URL}/api/tracksproxy/remove`;
  const response = await fetchWithAuth(url, {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ids: Array.isArray(trackIds) ? trackIds : [trackIds] })
  });
  if (!response.ok) {
    throw new Error(`Error removing saved tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const checkSavedTracks = async (trackIds) => {
  const ids = Array.isArray(trackIds) ? trackIds.join(',') : trackIds;
  const url = `${BASE_URL}/api/tracksproxy/check-saved?ids=${ids}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', 
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error checking saved tracks: ${response.statusText}`);
  }
  return await response.json();
};

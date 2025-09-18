export const LoginService_BASE_URL = 'http://localhost:5000';

export async function fetchWithAuth(url, options) {
  let token = localStorage.getItem('spotify_access_token');
  if (!token) {
    try {
      const data = await refreshToken();
      token = data.token || data;
      if (token) localStorage.setItem('spotify_access_token', token);
    } catch {
    }
  }
  options.headers = { ...options.headers, ...(token ? { Authorization: `Bearer ${token}` } : {}) };
  
  let response = await fetch(url, options);
  
  if (response.status === 401) {
    try {
      const data = await refreshToken();
      token = data.token || data;
      if (token) {
        localStorage.setItem('spotify_access_token', token);
        options.headers.Authorization = `Bearer ${token}`;
        response = await fetch(url, options);
      }
    } catch {

    }
  }
  return response;
}

export const TopItemsService_getTopTracks = async (timeSpan) => {
  const url = `${LoginService_BASE_URL}/api/toptracks/${timeSpan}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching top tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const TopItemsService_getTopArtists = async (timeSpan) => {
  const url = `${LoginService_BASE_URL}/api/topartists/${timeSpan}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching top artists: ${response.statusText}`);
  }
  return await response.json();
};

export const refreshToken = async () => {
  const response = await fetch(`${LoginService_BASE_URL}/api/login/refresh`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error refreshing token: ${response.statusText}`);
  }
  const data = await response.json();
  return data;
};

export const getRecentlyPlayed = async (limit = 50, after = null, before = null) => {
  const queryParams = new URLSearchParams({ limit: limit.toString() });
  if (after) queryParams.append('after', after.toString());
  if (before) queryParams.append('before', before.toString());
  
  const url = `${LoginService_BASE_URL}/api/recentlyplayed/recent-tracks?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching recently played: ${response.statusText}`);
  }
  return await response.json();
};

export const getTrack = async (id, market = null) => {
  const queryParams = new URLSearchParams();
  if (market) queryParams.append('market', market);
  
  const url = `${LoginService_BASE_URL}/api/tracksproxy/${id}${queryParams.toString() ? `?${queryParams}` : ''}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching track: ${response.statusText}`);
  }
  return await response.json();
};

export const getSeveralTracks = async (ids, market = null) => {
  const queryParams = new URLSearchParams({ ids: Array.isArray(ids) ? ids.join(',') : ids });
  if (market) queryParams.append('market', market);
  
  const url = `${LoginService_BASE_URL}/api/tracksproxy/several?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching several tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const getSavedTracks = async (limit = 20, offset = 0, market = null) => {
  const queryParams = new URLSearchParams({ 
    limit: limit.toString(), 
    offset: offset.toString() 
  });
  if (market) queryParams.append('market', market);
  
  const url = `${LoginService_BASE_URL}/api/tracksproxy/saved?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching saved tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const saveTracks = async (trackIds) => {
  const url = `${LoginService_BASE_URL}/api/tracksproxy/save`;
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
  const url = `${LoginService_BASE_URL}/api/tracksproxy/remove`;
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
  const url = `${LoginService_BASE_URL}/api/tracksproxy/check-saved?ids=${ids}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error checking saved tracks: ${response.statusText}`);
  }
  return await response.json();
};

export const getUserProfile = async () => {
  const url = `${LoginService_BASE_URL}/api/userproxy/profile`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
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
  
  const url = `${LoginService_BASE_URL}/api/userproxy/following/artists?${queryParams}`;
  const response = await fetchWithAuth(url, {
    method: 'GET', headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error(`Error fetching followed artists: ${response.statusText}`);
  }
  return await response.json();
};

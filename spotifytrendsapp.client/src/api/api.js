const LoginService_BASE_URL = 'http://localhost:5000';

/**
 * Wrapper to include Authorization header and auto-refresh JWT on 401.
 */
async function fetchWithAuth(url, options) {
  // get current token
  let token = localStorage.getItem('spotify_access_token');
  // initial refresh if none
  if (!token) {
    try {
      const data = await refreshToken();
      token = data.token || data;
      if (token) localStorage.setItem('spotify_access_token', token);
    } catch {
      // ignore refresh failure here
    }
  }
  // attach auth header
  options.headers = { ...options.headers, ...(token ? { Authorization: `Bearer ${token}` } : {}) };
  let response = await fetch(url, options);
  // on 401, attempt refresh and retry once
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
      // ignore
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

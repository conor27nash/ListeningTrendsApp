const BASE_URL = 'http://localhost:5000';

async function fetchWithAuth(url, options = {}) {
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

async function refreshToken() {
  const response = await fetch(`${BASE_URL}/api/login/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  });
  if (!response.ok) {
    throw new Error('Failed to refresh token');
  }
  return await response.json();
}

export { BASE_URL, fetchWithAuth, refreshToken };

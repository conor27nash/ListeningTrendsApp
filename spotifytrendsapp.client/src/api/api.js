const LoginService_BASE_URL = 'http://localhost:5000';

export const TopItemsService_getTopTracks = async (timeSpan) => {
  try {
    const response = await fetch(`${LoginService_BASE_URL}/api/toptracks/${timeSpan}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });
     
    if (!response.ok) {
      throw new Error(`Error fetching top tracks: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error fetching top tracks:', error);
    throw error;
  }
};

export const TopItemsService_getTopArtists = async (timeSpan) => {
  try {
    const response = await fetch(`${LoginService_BASE_URL}/api/topartists/${timeSpan}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });
     if (!response.ok) {
       throw new Error(`Error fetching top artists: ${response.statusText}`);
     }
    return await response.json();
  } catch (error) {
    console.error('Error fetching top artists:', error);
    throw error;
  }
};

export const refreshToken = async () => {
  try {
    const response = await fetch(`${LoginService_BASE_URL}/api/login/refresh`, {
       method: 'POST',
       headers: { 'Content-Type': 'application/json' },
    });
     if (!response.ok) {
       throw new Error(`Error refreshing token: ${response.statusText}`);
     }
    return await response.json();
  } catch (error) {
    console.error('Error refreshing token:', error);
    throw error;
  }
};

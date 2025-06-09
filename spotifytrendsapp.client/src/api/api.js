const API_BASE_URL = 'http://localhost:5000/api';

export const getTopTracks = async (timeSpan = 'medium_term') => {
  try {
    const response = await fetch(`${API_BASE_URL}/topitems/top-tracks/${timeSpan}`);
    if (!response.ok) {
      throw new Error(`Error fetching top tracks: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Error fetching top tracks:', error);
    throw error;
  }
};

export const getTopArtists = async (timeSpan = 'medium_term') => {
  try {
    const response = await fetch(`${API_BASE_URL}/topitems/top-artists/${timeSpan}`);
    if (!response.ok) {
      throw new Error(`Error fetching top artists: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Error fetching top artists:', error);
    throw error;
  }
};

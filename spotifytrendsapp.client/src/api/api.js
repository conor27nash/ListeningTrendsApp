const TopItemsService_BASE_URL = 'http://localhost:5002/api';
const LoginService_BASE_URL = 'http://localhost:5000';

// const getAuthHeader = async () => {
//   let token = localStorage.getItem('spotify_access_token');

//   if (!token) {
//     token = await refreshToken();
//     if (token) {
//       localStorage.setItem('spotify_access_token', token);
//     }
//   }

//   return token ? { Authorization: `Bearer ${token}` } : {};
// };

// const refreshToken = async () => {
//   try {
//     const response = await fetch(LoginService_BASE_URL + "/refresh", {
//       method: "POST",
//       headers: {
//         "Content-Type": "application/json",
//       },
//     });

//     if (!response.ok) {
//       throw new Error(`Error refreshing token: ${response.statusText}`);
//     }

//     const data = await response.json();
//     return data.token;
//   } catch (error) {
//     console.error("Error refreshing token:", error);
//     return null;
//   }
// };

export const TopItemsService_getTopTracks = async (timeSpan = 'medium_term') => {
  try {
    const response = await fetch(`${LoginService_BASE_URL}/toptracks?timeSpan=${timeSpan}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
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

export const TopItemsService_getTopArtists = async (timeSpan = 'medium_term') => {
  try {
    const response = await fetch(`${TopItemsService_BASE_URL}/topartists/top-artists/${timeSpan}`, {
      headers: {
        ...(await getAuthHeader()),
      },
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

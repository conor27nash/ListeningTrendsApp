import { fetchWithAuth, LoginService_BASE_URL } from './api';

/**
 * Fetches aggregated analytics data for the dashboard
 * @param {string} timeRange - Spotify time range (short_term, medium_term, long_term)
 */
export const getAnalytics = async (timeRange = 'medium_term') => {
  const url = `${LoginService_BASE_URL}/api/analytics/analytics?timeRange=${timeRange}`;
  const response = await fetchWithAuth(url, { method: 'GET' });
  if (!response.ok) {
    throw new Error(`Error fetching analytics: ${response.statusText}`);
  }
  return await response.json();
};

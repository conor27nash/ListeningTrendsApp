import { BASE_URL, fetchWithAuth } from './base.js';

/**
 * Authentication API functions
 */

export const login = async () => {
  window.location.href = `${BASE_URL}/api/login`;
};

export const logout = async () => {
  localStorage.removeItem('spotify_access_token');
  window.location.href = '/';
};

export const getAuthStatus = async () => {
  const token = localStorage.getItem('spotify_access_token');
  return !!token;
};

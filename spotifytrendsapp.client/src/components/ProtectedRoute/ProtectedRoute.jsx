import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';

export default function ProtectedRoute({ children }) {
  const token = localStorage.getItem('spotify_access_token');
  const location = useLocation();
  if (!token) {
    // Redirect to home/login page, preserve attempted URL
    return <Navigate to="/" state={{ from: location }} replace />;
  }
  return children;
}

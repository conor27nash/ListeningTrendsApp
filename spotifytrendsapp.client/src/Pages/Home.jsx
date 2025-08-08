import React, { useEffect, useState } from 'react';
import Login from '../Login/Login';
import Dashboard from '../components/Dashboard/Dashboard';

export default function Home() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    useEffect(() => {
        const token = localStorage.getItem('spotify_access_token');
        setIsAuthenticated(!!token);
    }, []);
    
    return (
        <div>
            {!isAuthenticated ? (
                <div style={{ 
                    display: 'flex', 
                    flexDirection: 'column', 
                    alignItems: 'center', 
                    justifyContent: 'center', 
                    minHeight: '100vh',
                    background: 'linear-gradient(135deg, #0f0f23 0%, #1a1a3a 100%)',
                    color: '#ffffff'
                }}>
                    <div style={{ textAlign: 'center', marginBottom: '40px' }}>
                        <h1 style={{ 
                            fontSize: '3rem', 
                            fontWeight: '700',
                            background: 'linear-gradient(45deg, #1db954, #1ed760)',
                            WebkitBackgroundClip: 'text',
                            WebkitTextFillColor: 'transparent',
                            backgroundClip: 'text',
                            marginBottom: '20px'
                        }}>
                            Spotify Trends
                        </h1>
                        <p style={{ 
                            fontSize: '1.2rem', 
                            opacity: '0.8',
                            maxWidth: '600px',
                            lineHeight: '1.6'
                        }}>
                            Discover insights about your music taste with beautiful analytics and visualizations
                        </p>
                        <h3 style={{ marginTop: '2rem', fontSize: '1.5rem', color: '#ffffff' }}>
                            Please log in with Spotify to view your personalized analytics
                        </h3>
                    </div>
                    {/* <div style={{ marginTop: '1rem' }}>
                        <Login onLoginSuccess={setIsAuthenticated} />
                    </div> */}
                </div>
            ) : (
                <Dashboard />
            )}
        </div>
    );
}

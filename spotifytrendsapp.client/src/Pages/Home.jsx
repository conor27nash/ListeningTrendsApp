import React from 'react';
import { useEffect, useState } from 'react';
import Login from '../Login/Login';


export default function Home() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    useEffect(() => {
        // Check if a code exists in localStorage
        const code = localStorage.getItem('code');
        if (code) {
            setIsAuthenticated(true);
        }
    }, []);
    
    
    return (

        <div>
            <h2>Home Page</h2>;
            <div className="App">
                <h1>Spotify Trends</h1>
                <p>This component demonstrates fetching data from the server.</p>
            </div><Login onLoginSuccess={() => setIsAuthenticated(true)} />
            {!isAuthenticated ? (
                <></>
            ) : (
                <>
                    <h1>Welcome to the app</h1>
                </>
            )}
        </div>
    )
}
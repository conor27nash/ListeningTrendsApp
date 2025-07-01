import './Login.css';
import { useEffect, useState } from 'react';

function Login({ onLoginSuccess }) {

    const [isLoggedIn, setIsLoggedIn] = useState(() => !!localStorage.getItem('spotify_access_token'));

    const LOGIN_CONNECT_URL = `${window.location.origin}/api/login/connect`;

    useEffect(() => {
        const params = new URLSearchParams(window.location.search);
        const tokenFromUrl = params.get("token");
        if (tokenFromUrl && !localStorage.getItem("tokenPosted")) {
            localStorage.setItem('spotify_access_token', tokenFromUrl);
            localStorage.setItem('tokenPosted', 'true');
            setIsLoggedIn(true);
            onLoginSuccess(true);

            window.history.replaceState({}, document.title, window.location.pathname);
        }
    }, [onLoginSuccess]);

    const logout = () => {
        localStorage.removeItem('spotify_access_token');
        localStorage.removeItem('tokenPosted');
        setIsLoggedIn(false);
        onLoginSuccess(false);
    }

    return (
        <div className="App">
            <header className="App-header">
                <h1>Spotify React</h1>
                {/* Show login link until user is authenticated */}
                {!isLoggedIn ? (
                    <a href={LOGIN_CONNECT_URL}>
                        Login with Spotify
                    </a>
                ) : (
                    <div>
                        <h2>Logged in</h2>
                        <button onClick={logout}>Logout</button>
                    </div>
                )}
            </header>
        </div>
    );
}    

export default Login;
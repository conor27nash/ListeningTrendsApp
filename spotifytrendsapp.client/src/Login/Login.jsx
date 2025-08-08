import './Login.css';
import { useEffect, useState } from 'react';

function Login({ onLoginSuccess = () => {} }) {

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
        // Redirect to Home page after logout
        window.location.href = '/';
    }

    // Compose login/logout content
    const content = !isLoggedIn ? (
        <button className="login-button" onClick={() => window.location.href = LOGIN_CONNECT_URL}>
            Login with Spotify
        </button>
    ) : (
        <div className="logout-section">
            <h2>You are logged in</h2>
            <button className="logout-button" onClick={logout}>Logout</button>
        </div>
    );
    // Always render inline login
    return <div>{content}</div>;
}    

export default Login;
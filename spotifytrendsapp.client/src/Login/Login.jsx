import './Login.css';
import { useEffect, useState } from 'react';

function Login() {
    const [code, setCode] = useState("")
    const CLIENT_ID = "4c4f2ecd3bf648d49adc7846d0091831"; // Replace with your Spotify client ID
    const REDIRECT_URI = "http://localhost:5173/";
    const AUTH_ENDPOINT = "https://accounts.spotify.com/authorize";
    const RESPONSE_TYPE = "code";
    const SCOPES = 'user-top-read'

    useEffect(() => {
        console.log("useEffect triggered");
        const queryParams = new URLSearchParams(window.location.search);
        const codeFromUrl = queryParams.get("code");

        if (codeFromUrl && !window.localStorage.getItem("codePosted")) {
            console.log("Code received: ", codeFromUrl);
            window.localStorage.setItem("code", codeFromUrl);
            window.localStorage.setItem("codePosted", "true");
            setCode(codeFromUrl);
            postCode(codeFromUrl);
        } else if (window.localStorage.getItem("codePosted")) {
            console.log("Code already posted, skipping postCode");
        }
    }, []);

    const logout = () => {
        setCode("")
        window.localStorage.removeItem("code")
    }

    const postCode = async (_code) => {
        try {
            const data = { code: _code };
            const response = await fetch("/api/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });
            const apiResponse = await response.json();
            console.log("API Response:", apiResponse);
        } catch (error) {
            console.error("Error sending data:", error);
        }
    }

    return (
        <div className="App">
            <header className="App-header">
                <h1>Spotify React</h1>
                {!code ? (
                    <a
                        href={`${AUTH_ENDPOINT}?client_id=${CLIENT_ID}&redirect_uri=${REDIRECT_URI}&response_type=${RESPONSE_TYPE}&scope=${SCOPES}`}
                    >
                        Login to Spotify
                    </a>
                ) : (
                    <div>
                        <h2>Logged in</h2>
                        <p>Token: {code}</p>
                        <button onClick={logout}>Logout</button>
                    </div>
                )}
            </header>
        </div>
    );
}

export default Login;
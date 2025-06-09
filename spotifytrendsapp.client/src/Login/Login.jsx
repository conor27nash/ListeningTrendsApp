import './Login.css';
import { useEffect, useState } from 'react';

function Login() {
    const [token, setToken] = useState("")
    const CLIENT_ID = "4c4f2ecd3bf648d49adc7846d0091831"; // Replace with your Spotify client ID
    const REDIRECT_URI = "https://localhost:5173/";
    const AUTH_ENDPOINT = "https://accounts.spotify.com/authorize";
    const RESPONSE_TYPE = "token";

    useEffect(async () => {
        const hash = window.location.hash
        let token = window.localStorage.getItem("token")

        if (!token && hash) {
            token = hash.substring(1).split("&").find(elem => elem.startsWith("access_token")).split("=")[1]

            window.location.hash = ""
            window.localStorage.setItem("token", token)
        }

        await setToken(token)
        await postToken(token)

    }, [])

    const logout = () => {
        setToken("")
        window.localStorage.removeItem("token")
    }

    const postToken = async (_token) => {
        try {
            const data ={token: _token}
            const response = await fetch("login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });
            const apiResponse = await response.json()
            console.log(apiResponse)

        } catch (error) {
            console.error("Error sending data:", error);
        }

    }

    return (
        <div className="App">
            <header className="App-header">
                <h1>Spotify React</h1>
                {!token ?
                    <a href={`${AUTH_ENDPOINT}?client_id=${CLIENT_ID}&redirect_uri=${REDIRECT_URI}&response_type=${RESPONSE_TYPE}`}>Login
                        to Spotify</a>
                    : <div>
                        <h2>Logged in</h2>
                        <p>Token: {token}</p>
                        <button onClick={logout}>Logout</button>
                    </div>}
            </header>
        </div>
    );
}

export default Login;
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import './App.css';
import Login from './Login/Login.jsx';
import TopTracks from './components/TopTracks';
import TopArtists from './components/TopArtists';

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={
                    <div>
                        <div className="App">
                            <h1>Spotify Trends</h1>
                            <p>This component demonstrates fetching data from the server.</p>
                        </div>
                        <Login />
                        <TopTracks />
                        <TopArtists />
                    </div>
                } />
                <Route path="/callback" element={<Navigate to="/" />} />
            </Routes>
        </Router>
    );
}

export default App;
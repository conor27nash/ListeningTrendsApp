import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import './App.css';
import Sidebar from './components/SideBar/Sidebar.jsx';
import TopTracksPage from './Pages/TopTracks.jsx';
import Home from './Pages/Home';
import TopArtistsPage from './Pages/TopArtistsPage';
import RecentlyPlayedPage from './Pages/RecentlyPlayed.jsx';
import ProtectedRoute from './components/ProtectedRoute/ProtectedRoute';

function App() {
    return (
        <Router>
            <div className="app-layout">
                <Sidebar />
                <div className="app-content">
                    <Routes>
                        <Route path="/" element={<Home />} />
                        <Route path="/top-tracks" element={<ProtectedRoute><TopTracksPage /></ProtectedRoute>} />
                        <Route path="/top-artists" element={<ProtectedRoute><TopArtistsPage /></ProtectedRoute>} />
                        <Route path="/recent-played" element={<ProtectedRoute><RecentlyPlayedPage /></ProtectedRoute>} />
                    </Routes>
                </div>
            </div>
        </Router>
    );
}

export default App;
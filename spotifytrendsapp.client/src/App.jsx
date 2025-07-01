import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import './App.css';
import Sidebar from './components/SideBar/Sidebar.jsx';
import TopTracksPage from './Pages/TopTracks.jsx';
import Home from './Pages/Home';
import TopArtistsPage from './Pages/TopArtistsPage';
import RecentlyPlayedPage from './Pages/RecentlyPlayed.jsx';
import WrappedPage from './Pages/WrappedPage.jsx';

function App() {


    return (
        <Router>
            <Sidebar />
            <div style={{ marginLeft: '220px', padding: '1rem' }}>
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/top-tracks" element={<TopTracksPage />} />
                    <Route path="/top-artists" element={<TopArtistsPage />} />
                    <Route path="/recent-played" element={<RecentlyPlayedPage />} />
                    <Route path="/wrapped" element={<WrappedPage />} />
                </Routes>
            </div>
        </Router>
    );


}

export default App;
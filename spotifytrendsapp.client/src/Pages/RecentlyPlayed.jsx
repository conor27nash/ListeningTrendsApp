import React, { useState, useEffect } from 'react';
import { getRecentlyPlayed } from '../api/recentlyPlayed';
import TrackCard from '../components/TrackListing/TrackListingCard';
import './RecentlyPlayed.css';

const RecentlyPlayedPage = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const [recentTracks, setRecentTracks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [cursors, setCursors] = useState({ after: null, before: null });
  const itemsPerPage = 10;

  useEffect(() => {
    loadRecentTracks();
  }, []);

  const loadRecentTracks = async (after = null, before = null) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getRecentlyPlayed(50, after, before);
      setRecentTracks(data.items || []);
      setCursors(data.cursors || { after: null, before: null });
      setCurrentPage(1);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Loading recently played tracks...</div>;
  if (error) return <div>Error loading tracks: {error}</div>;

  const totalItems = recentTracks.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);

  const startIdx = (currentPage - 1) * itemsPerPage;
  const currentTracks = recentTracks.slice(startIdx, startIdx + itemsPerPage);

  return (
    <div className="recently-played-page">
      <h2>Recently Played</h2>
      <div className="navigation-buttons">
        <button onClick={() => loadRecentTracks()} disabled={loading}>
          🔄 Refresh
        </button>
      </div>
      <div className="tracks-grid">
        {currentTracks.map((item, index) => {
          const playedAt = new Date(item.played_at);
          return (
            <div key={`${item.track.id}-${item.played_at}`} className="recent-card">
              <TrackCard track={item.track} position={startIdx + index} />
              <p className="played-at">
                Played at: {playedAt.toLocaleString()}
              </p>
            </div>
          );
        })}
      </div>
      <div className="pagination">
        <button disabled={currentPage === 1} onClick={() => setCurrentPage(1)}>⏮ First</button>
        <button disabled={currentPage === 1} onClick={() => setCurrentPage(p => p - 1)}>◀ Previous</button>
        <span>Page {currentPage} of {totalPages}</span>
        <button disabled={currentPage === totalPages} onClick={() => setCurrentPage(p => p + 1)}>Next ▶</button>
        <button disabled={currentPage === totalPages} onClick={() => setCurrentPage(totalPages)}>Last ⏭</button>
      </div>
    </div>
  );
};

export default RecentlyPlayedPage;

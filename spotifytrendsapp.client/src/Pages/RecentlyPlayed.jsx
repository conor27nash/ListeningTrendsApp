import React, { useState } from 'react';
import tracksData from '../PlaceholderJson/Toptracks.json';
import TrackCard from '../components/TrackListing/TrackListingCard';
import './RecentlyPlayed.css';

const RecentlyPlayedPage = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  const totalItems = tracksData.items.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);

  const startIdx = (currentPage - 1) * itemsPerPage;
  const currentTracks = tracksData.items.slice(startIdx, startIdx + itemsPerPage);

  // Generate mock "played at" timestamps (pretend they were played 5 minutes apart)
  const now = new Date();
  const addMinutes = (date, minutes) => new Date(date.getTime() - minutes * 60000);

  return (
    <div className="recently-played-page">
      <h2>Recently Played</h2>
      <div className="tracks-grid">
        {currentTracks.map((track, index) => {
          const playedAt = addMinutes(now, startIdx * 5 + index * 5);
          return (
            <div key={track.id} className="recent-card">
              <TrackCard track={track} position={startIdx + index} />
              <p className="played-at">Played at: {playedAt.toLocaleTimeString()}</p>
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

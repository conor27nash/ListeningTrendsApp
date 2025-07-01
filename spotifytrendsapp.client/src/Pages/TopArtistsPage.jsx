import React, { useState } from 'react';
import artistData from '../PlaceholderJson/TopArtists.json';
import TimeRangeToggle from '../components/TimeRangeToggle/TimeRangeToggle';
import ArtistCard from '../components/ArtistListing/ArtistCard';
import RecommendationModal from '../components/RecommendationModal/RecommendationModal';
import './TopArtistsPage.css';

const TopArtistsPage = () => {
  const [range, setRange] = useState('short');
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedArtist, setSelectedArtist] = useState(null);

  const itemsPerPage = 10;
  const totalItems = artistData.items.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);

  const startIdx = (currentPage - 1) * itemsPerPage;
  const currentArtists = artistData.items.slice(startIdx, startIdx + itemsPerPage);

  const openModal = (artist) => setSelectedArtist(artist);
  const closeModal = () => setSelectedArtist(null);

  const recommendedArtists = artistData.items
    .filter(a => a.id !== selectedArtist?.id)
    .slice(0, 4);

  return (
    <div className="top-artists-page">
      <TimeRangeToggle current={range} onChange={setRange} />
      <div className="artists-grid">
        {currentArtists.map((artist, index) => (
          <ArtistCard
            key={artist.id}
            artist={artist}
            position={startIdx + index}
            onClick={() => openModal(artist)}
          />
        ))}
      </div>
      <div className="pagination">
        <button disabled={currentPage === 1} onClick={() => setCurrentPage(1)}>⏮ First</button>
        <button disabled={currentPage === 1} onClick={() => setCurrentPage(p => p - 1)}>◀ Previous</button>
        <span>Page {currentPage} of {totalPages}</span>
        <button disabled={currentPage === totalPages} onClick={() => setCurrentPage(p => p + 1)}>Next ▶</button>
        <button disabled={currentPage === totalPages} onClick={() => setCurrentPage(totalPages)}>Last ⏭</button>
      </div>

      <RecommendationModal
        isOpen={!!selectedArtist}
        onClose={closeModal}
        title="Artist Info"
        mainItem={selectedArtist}
        recommendations={recommendedArtists}
        isTrack={false}
      />
    </div>
  );
};

export default TopArtistsPage;
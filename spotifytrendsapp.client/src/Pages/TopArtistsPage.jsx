import React, { useState, useEffect } from 'react';
import { TopItemsService_getTopArtists } from '../api/api';
import TimeRangeToggle from '../components/TimeRangeToggle/TimeRangeToggle';
import ArtistCard from '../components/ArtistListing/ArtistCard';
import RecommendationModal from '../components/RecommendationModal/RecommendationModal';
import './TopArtistsPage.css';

const TopArtistsPage = () => {
  const [range, setRange] = useState('short_term');
  const [artistsData, setArtistsData] = useState({ items: [] });
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedArtist, setSelectedArtist] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    TopItemsService_getTopArtists(range)
      .then(data => {
        setArtistsData({ items: data.items });
        setCurrentPage(1);
      })
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [range]);

  if (loading) return <div>Loading top artists...</div>;
  if (error) return <div>Error loading artists: {error}</div>;

  const itemsPerPage = 10;
  const totalItems = artistsData.items.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);

  const startIdx = (currentPage - 1) * itemsPerPage;
  const currentArtists = artistsData.items.slice(startIdx, startIdx + itemsPerPage);

  const openModal = (artist) => setSelectedArtist(artist);
  const closeModal = () => setSelectedArtist(null);

  const recommendedArtists = artistsData.items
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
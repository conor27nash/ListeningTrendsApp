import React, { useState, useEffect } from 'react';
import { TopItemsService_getTopTracks } from '../api/api';
import TimeRangeToggle from '../components/TimeRangeToggle/TimeRangeToggle';
import TrackCard from '../components/TrackListing/TrackListingCard';
import RecommendationModal from '../components/RecommendationModal/RecommendationModal';
import './TopTracks.css';

const TopTracksPage = () => {
  const [range, setRange] = useState('short_term');
  const [tracksData, setTracksData] = useState({ items: [] });
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedTrack, setSelectedTrack] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    setError(null);
    TopItemsService_getTopTracks(range)
      .then(data => {
        setTracksData({ items: data.items });
        setCurrentPage(1);
      })
      .catch(err => setError(err.message))
      .finally(() => setLoading(false));
  }, [range]);

  if (loading) return <div>Loading top tracks...</div>;
  if (error) return <div>Error loading tracks: {error}</div>;

  const itemsPerPage = 10;
  const totalItems = tracksData.items.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);

  const startIdx = (currentPage - 1) * itemsPerPage;
  const currentTracks = tracksData.items.slice(startIdx, startIdx + itemsPerPage);

  const openModal = (track) => setSelectedTrack(track);
  const closeModal = () => setSelectedTrack(null);

  const recommendedTracks = tracksData.items
    .filter(t => t.id !== selectedTrack?.id)
    .slice(0, 4);

  return (
    <div className="top-tracks-page">
      <TimeRangeToggle current={range} onChange={setRange} />
      <div className="tracks-grid">
        {currentTracks.map((track, index) => (
          <TrackCard
            key={track.id}
            track={track}
            position={startIdx + index}
            onClick={() => openModal(track)}
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
        isOpen={!!selectedTrack}
        onClose={closeModal}
        title="Track Info"
        mainItem={selectedTrack}
        recommendations={recommendedTracks}
        isTrack={true}
      />
    </div>
  );
};

export default TopTracksPage;

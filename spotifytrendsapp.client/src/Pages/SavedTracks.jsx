import React, { useState, useEffect } from 'react';
import { getSavedTracks, saveTracks, removeSavedTracks, checkSavedTracks } from '../api/tracks';
import TrackCard from '../components/TrackListing/TrackListingCard';
import './SavedTracks.css';

const SavedTracksPage = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const [savedTracks, setSavedTracks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [totalTracks, setTotalTracks] = useState(0);
  const [actionLoading, setActionLoading] = useState(false);
  const itemsPerPage = 20;

  useEffect(() => {
    loadSavedTracks();
  }, [currentPage]);

  const loadSavedTracks = async () => {
    setLoading(true);
    setError(null);
    try {
      const offset = (currentPage - 1) * itemsPerPage;
      const data = await getSavedTracks(itemsPerPage, offset);
      setSavedTracks(data.items || []);
      setTotalTracks(data.total || 0);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveTrack = async (trackId, trackName) => {
    if (!confirm(`Are you sure you want to remove "${trackName}" from your saved tracks?`)) {
      return;
    }

    setActionLoading(true);
    try {
      await removeSavedTracks([trackId]);
      // Refresh the current page
      await loadSavedTracks();
    } catch (err) {
      setError(`Error removing track: ${err.message}`);
    } finally {
      setActionLoading(false);
    }
  };

  const handleRefresh = () => {
    loadSavedTracks();
  };

  if (loading) return <div className="loading">Loading saved tracks...</div>;
  if (error) return <div className="error">Error loading tracks: {error}</div>;

  const totalPages = Math.ceil(totalTracks / itemsPerPage);

  return (
    <div className="saved-tracks-page">
      <h2>Your Saved Tracks</h2>
      
      <div className="track-info">
        <p>Total saved tracks: {totalTracks}</p>
        <div className="navigation-buttons">
          <button onClick={handleRefresh} disabled={loading || actionLoading}>
            🔄 Refresh
          </button>
        </div>
      </div>

      {savedTracks.length === 0 ? (
        <div className="no-tracks">
          <p>You haven't saved any tracks yet.</p>
          <p>Start exploring your music and save your favorite tracks!</p>
        </div>
      ) : (
        <>
          <div className="tracks-grid">
            {savedTracks.map((item, index) => {
              const addedAt = new Date(item.added_at);
              const offset = (currentPage - 1) * itemsPerPage;
              return (
                <div key={item.track.id} className="saved-track-card">
                  <TrackCard track={item.track} position={offset + index} showSaveButton={false} />
                  <div className="track-actions">
                    <p className="added-at">
                      Added: {addedAt.toLocaleDateString()}
                    </p>
                    <button
                      className="remove-btn"
                      onClick={() => handleRemoveTrack(item.track.id, item.track.name)}
                      disabled={actionLoading}
                    >
                      {actionLoading ? '⏳' : '🗑️'} Remove
                    </button>
                  </div>
                </div>
              );
            })}
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button 
                disabled={currentPage === 1 || loading || actionLoading} 
                onClick={() => setCurrentPage(1)}
              >
                ⏮ First
              </button>
              <button 
                disabled={currentPage === 1 || loading || actionLoading} 
                onClick={() => setCurrentPage(p => p - 1)}
              >
                ◀ Previous
              </button>
              <span>Page {currentPage} of {totalPages}</span>
              <button 
                disabled={currentPage === totalPages || loading || actionLoading} 
                onClick={() => setCurrentPage(p => p + 1)}
              >
                Next ▶
              </button>
              <button 
                disabled={currentPage === totalPages || loading || actionLoading} 
                onClick={() => setCurrentPage(totalPages)}
              >
                Last ⏭
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default SavedTracksPage;

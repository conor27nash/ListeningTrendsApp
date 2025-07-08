import React, { useState, useEffect } from 'react';
import { saveTracks, removeSavedTracks, checkSavedTracks } from '../../api/api';
import './SaveTrackButton.css';

const SaveTrackButton = ({ trackId, trackName, onSaveStatusChange }) => {
  const [isSaved, setIsSaved] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    checkIfSaved();
  }, [trackId]);

  const checkIfSaved = async () => {
    try {
      const savedStatus = await checkSavedTracks([trackId]);
      setIsSaved(savedStatus[0] || false);
    } catch (err) {
      console.error('Error checking saved status:', err);
    }
  };

  const handleToggleSave = async () => {
    if (loading) return;
    
    setLoading(true);
    setError(null);
    
    try {
      if (isSaved) {
        await removeSavedTracks([trackId]);
        setIsSaved(false);
        onSaveStatusChange && onSaveStatusChange(trackId, false);
      } else {
        await saveTracks([trackId]);
        setIsSaved(true);
        onSaveStatusChange && onSaveStatusChange(trackId, true);
      }
    } catch (err) {
      setError(`Error ${isSaved ? 'removing' : 'saving'} track: ${err.message}`);
      console.error('Error toggling save status:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="save-track-button">
      <button
        className={`heart-btn ${isSaved ? 'saved' : ''}`}
        onClick={handleToggleSave}
        disabled={loading}
        title={isSaved ? `Remove "${trackName}" from saved tracks` : `Save "${trackName}" to your library`}
      >
        {loading ? '⏳' : isSaved ? '❤️' : '🤍'}
      </button>
      {error && <div className="error-message">{error}</div>}
    </div>
  );
};

export default SaveTrackButton;

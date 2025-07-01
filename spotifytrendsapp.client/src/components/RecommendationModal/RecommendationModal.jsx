import React from 'react';
import './RecommendationModal.css';

const RecommendationModal = ({ isOpen, onClose, title, mainItem, recommendations, isTrack }) => {
  if (!isOpen || !mainItem) return null;

  const getSpotifyLink = (item) => item.external_urls?.spotify;
  const getName = (item) => item.name;

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>×</button>
        <h2>{title}</h2>

        <div className="modal-body">
          <div className="modal-main-item">
            <img
              src={isTrack ? mainItem.album.images[1]?.url : mainItem.images[1]?.url}
              alt={getName(mainItem)}
            />
            <div className="modal-main-info">
              <h3>{getName(mainItem)}</h3>
              <p>
                {isTrack
                  ? mainItem.artists.map(a => a.name).join(', ')
                  : mainItem.genres.slice(0, 2).join(', ')
                }
              </p>
            </div>
          </div>

          <h4>Recommended:</h4>
          <ul className="modal-recommendations-list">
            {recommendations.map((item, index) => (
              <li key={item.id + '_rec'}>
                <span>{index + 1}. {getName(item)}</span>
                <a href={getSpotifyLink(item)} target="_blank" rel="noreferrer">
                  Open in Spotify
                </a>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default RecommendationModal;

import React from 'react';
import './TrackListingCard.css';

const TrackCard = ({ track, position, onClick = () => {} }) => {
  const imageUrl = track.album.images[1]?.url;
  const artistNames = track.artists.map(a => a.name).join(', ');

  return (
    <div className="track-card clickable" onClick={onClick}>
      <div className="track-rank">{position + 1}</div>
      <img src={imageUrl} alt={track.name} />
      <div className="track-info">
        <h4>{track.name}</h4>
        <p>{artistNames}</p>
        <p className="album">{track.album.name}</p>
      </div>
    </div>
  );
};

export default TrackCard;
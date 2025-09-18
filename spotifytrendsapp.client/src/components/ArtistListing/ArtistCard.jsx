import React from 'react';
import './ArtistCard.css';

const ArtistCard = ({ artist, position, onClick = () => {} }) => {
  const imageUrl = artist.images[1]?.url;
  const genres = artist.genres.slice(0, 2).join(', ');

  return (
    <div className="artist-card clickable" onClick={onClick}>
      <div className="artist-rank">{position + 1}</div>
      <img src={imageUrl} alt={artist.name} />
      <div className="artist-info">
        <h4>{artist.name}</h4>
        <p>{genres}</p>
        <p className="followers">{artist.followers.total.toLocaleString()} followers</p>
      </div>
    </div>
  );
};

export default ArtistCard;
import React, { useState, useEffect } from 'react';
import { getTopTracks } from '../api/api';

const TopTracks = () => {
  const [tracks, setTracks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTracks = async () => {
      try {
        const data = await getTopTracks();
        setTracks(data.items);
      } catch (err) {
        setError(err);
      } finally {
        setLoading(false);
      }
    };

    fetchTracks();
  }, []);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;

  return (
    <div>
      <h1>Top Tracks</h1>
      <ul>
        {tracks.map((track) => (
          <li key={track.id}>{track.name} by {track.artists.map(artist => artist.name).join(', ')}</li>
        ))}
      </ul>
    </div>
  );
};

export default TopTracks;

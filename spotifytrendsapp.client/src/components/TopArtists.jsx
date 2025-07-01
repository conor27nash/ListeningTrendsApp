import React, { useState, useEffect } from 'react';
import { TopItemsService_getTopArtists } from '../api/api';

const TopArtists = () => {
  const [artists, setArtists] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchArtists = async () => {
      try {
        const data = await TopItemsService_getTopArtists();
        setArtists(data.items);
      } catch (err) {
        setError(err);
      } finally {
        setLoading(false);
      }
    };

    fetchArtists();
  }, []);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;

  return (
    <div>
      <h1>Top Artists</h1>
      <ul>
        {artists.map((artist) => (
          <li key={artist.id}>{artist.name}</li>
        ))}
      </ul>
    </div>
  );
};

export default TopArtists;

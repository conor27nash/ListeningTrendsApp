import React from 'react';
import Highcharts from 'highcharts';
import HighchartsReact from 'highcharts-react-official';
import './WrappedPage.css';

const WrappedPage = () => {
  const genreData = [
    { name: 'Pop', y: 35 },
    { name: 'Rock', y: 25 },
    { name: 'Hip-Hop', y: 15 },
    { name: 'Indie', y: 10 },
    { name: 'Electronic', y: 15 }
  ];

  const monthlyData = {
    categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
    data: [20, 35, 40, 50, 60]
  };

  const topArtists = {
    categories: ['Artist A', 'Artist B', 'Artist C', 'Artist D', 'Artist E'],
    data: [60, 55, 50, 40, 30]
  };

  return (
    <div className="wrapped-page">
      <h2>Your 2025 Wrapped</h2>

      <div className="chart-block">
        <h3>Top Genres</h3>
        <HighchartsReact
          highcharts={Highcharts}
          options={{
            chart: { type: 'pie' },
            title: { text: null },
            series: [{ name: 'Genres', colorByPoint: true, data: genreData }]
          }}
        />
      </div>

      <div className="chart-block">
        <h3>Monthly Listening</h3>
        <HighchartsReact
          highcharts={Highcharts}
          options={{
            chart: { type: 'column' },
            xAxis: { categories: monthlyData.categories },
            title: { text: null },
            series: [{ name: 'Tracks Played', data: monthlyData.data }]
          }}
        />
      </div>

      <div className="chart-block">
        <h3>Top Artists</h3>
        <HighchartsReact
          highcharts={Highcharts}
          options={{
            chart: { type: 'bar' },
            xAxis: { categories: topArtists.categories },
            title: { text: null },
            series: [{ name: 'Plays', data: topArtists.data }]
          }}
        />
      </div>
    </div>
  );
};

export default WrappedPage;

import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';

// Stable colour from a string (UK spelling 💂)
function getGenreColour(genre) {
  if (!genre) return '#999';
  let hash = 0;
  for (let i = 0; i < genre.length; i++) {
    hash = genre.charCodeAt(i) + ((hash << 5) - hash);
  }
  const hue = Math.abs(hash) % 360;
  return `hsl(${hue}, 65%, 55%)`;
}

function percent(n, total) {
  if (!total) return '0%';
  return `${((n / total) * 100).toFixed(1)}%`;
}

function spotifySearchUrl(genre) {
  return `https://open.spotify.com/search/${encodeURIComponent(genre)}`;
}

export default function GenreBubble({ data = [], title = 'Genres (Bubble)' }) {
  const { points, total } = useMemo(() => {
    const arr = Array.isArray(data) ? data : [];
    const total = arr.reduce((s, g) => s + (g.count ?? 0), 0);

    const pts = arr.map(g => ({
      name: g.genre,
      value: g.count ?? 0,         // packed bubble uses "value"
      color: getGenreColour(g.genre),
      custom: { count: g.count ?? 0, genre: g.genre }
    }));

    // Sort largest first for nicer layout
    pts.sort((a, b) => (b.value - a.value));

    return { points: pts, total };
  }, [data]);

  const options = {
    chart: { 
      type: 'packedbubble', 
      backgroundColor: 'transparent', 
      height: '100%',
      spacingTop: 0,
      spacingBottom: 0,
      marginTop: 0,
      marginBottom: 0
    },
    title: { text: title },
    subtitle: {
      text: 'Bubble size = how often each genre appears.<br/><i>Which genres dominate my listening?</i>',
      style: { color: '#A0A0A0', fontSize: '12px' },
      useHTML: true
    },
    tooltip: {
      useHTML: true,
      formatter() {
        const cnt = this.point.custom?.count ?? this.point.value ?? 0;
        const clickHint = `<br/><span style="color:#1DB954;">Click to search on Spotify</span>`;
        return `<b>${this.point.name}</b><br/>Count: <b>${cnt}</b><br/>${clickHint}`;
      }
    },
    plotOptions: {
      packedbubble: {
        minSize: '60%',           
        maxSize: '180%',          
        zMin: 1,
        zMax: Math.max(1, ...points.map(p => p.value || 1)),
        layoutAlgorithm: {
          splitSeries: false,
          gravitationalConstant: 0.07,
          bubblePadding: 0,
          seriesInteraction: false
        },
        dataLabels: {
          enabled: true,
          style: { color: '#fff', textOutline: 'none', fontWeight: '600' },
          formatter() {
            const name = this.point.name || '';
            const short = name.length > 16 ? `${name.slice(0, 14)}…` : name;
            return short;
          }
        },
        cursor: 'pointer',
        point: {
          events: {
            click() {
              const url = spotifySearchUrl(this.name);
              if (url) window.open(url, '_blank', 'noopener');
            }
          }
        }
      }
    },
    series: [{
      name: 'Genres',
      data: points
    }],
    legend: { enabled: false },
    credits: { enabled: false }
  };

  return <HighchartsReact highcharts={Highcharts} options={options} />;
}

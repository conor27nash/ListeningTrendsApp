import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';

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

export default function TopGenresDonut({ data = [], title = 'Top 10 Genres (Share)' }) {
  const { top10, total } = useMemo(() => {
    const arr = Array.isArray(data) ? data : [];
    const totalCount = arr.reduce((s, g) => s + (g.count ?? 0), 0);

    const sorted = [...arr].sort((a, b) => (b.count ?? 0) - (a.count ?? 0));
    const top10 = sorted.slice(0, 10);
    const otherCount = sorted.slice(10).reduce((s, g) => s + (g.count ?? 0), 0);

    if (otherCount > 0) {
      top10.push({ genre: 'Other', count: otherCount });
    }

    const chartData = top10.map(g => ({
      name: g.genre,
      y: g.count ?? 0,
      color: g.genre === 'Other' ? '#ccc' : getGenreColour(g.genre),
      custom: { count: g.count ?? 0 },
      url: g.genre !== 'Other' ? spotifySearchUrl(g.genre) : null
    }));

    return { top10: chartData, total: totalCount };
  }, [data]);

  const options = {
    chart: { type: 'pie', backgroundColor: 'transparent', height: 520 },
    title: { text: title },
    subtitle: { 
      text: 'Share of listening from your 10 most-played genres.<br/><i>The bigger the slice, the more dominant that genre is in your listening mix.</i>',
      style: { color: '#A0A0A0', fontSize: '12px' },
      useHTML: true
    },
    tooltip: {
      useHTML: true,
      formatter() {
        const cnt = this.point.custom?.count ?? this.point.y ?? 0;
        const pct = percent(cnt, total);
        let html = `<b>${this.point.name}</b><br/>Count: <b>${cnt}</b><br/>Share: <b>${pct}</b>`;
        if (this.point.url) {
          html += `<br/><span style="color:#1DB954;">Click to view on Spotify</span>`;
        }
        return html;
      }
    },
    plotOptions: {
      pie: {
        innerSize: '50%',
        dataLabels: {
          enabled: true,
          format: '{point.name}: {point.percentage:.1f}%',
          style: { fontSize: '12px', textOutline: 'none', color: '#fff' }
        },
        point: {
          events: {
            mouseOver() {
              if (this.url) {
                document.body.style.cursor = 'pointer';
              }
            },
            mouseOut() {
              document.body.style.cursor = 'default';
            },
            click() {
              if (this.url) {
                window.open(this.url, '_blank', 'noopener');
              }
            }
          }
        }
      }
    },
    series: [{
      name: 'Genres',
      data: top10
    }],
    legend: { enabled: false },
    credits: { enabled: false }
  };

  return <HighchartsReact highcharts={Highcharts} options={options} />;
}

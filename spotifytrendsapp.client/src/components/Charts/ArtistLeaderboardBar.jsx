import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';
import { getArtistColour } from '../utils/artistColours';

const fmtFollowers = (n = 0) =>
  n >= 1e6 ? (n / 1e6).toFixed(1) + 'M' :
    n >= 1e3 ? (n / 1e3).toFixed(1) + 'k' :
      String(n);

export default function ArtistLeaderboardRanks({
  data,
  top,
  title = 'Artist Leaderboard (Positions)',
  description = 'Ordered by your score. #1 at the top. Bar length reflects position (not score).'
}) {
  const rows = useMemo(() => {
    const arr = Array.isArray(data) ? [...data] : [];
    arr.sort((a, b) => (b.rank ?? 0) - (a.rank ?? 0) || (b.popularity ?? 0) - (a.popularity ?? 0));
    const sliced = arr.slice(0, top);
    return sliced.map((r, i) => ({ ...r, position: i + 1 }));
  }, [data, top]);

  const N = rows.length;
  const seriesData = rows.map(r => ({
    y: N - r.position + 1,
    color: getArtistColour(r.artistName),
    url: r.spotifyLink ? `https://open.spotify.com/artist/${r.spotifyLink.split(':').pop()}` : null
  }));

  const options = {
    chart: { type: 'bar', height: 48 + N * 26, backgroundColor: 'transparent' },
    title: { text: title },
    subtitle: { text: description, style: { color: '#A0A0A0', fontSize: '12px' } },
    xAxis: {
      categories: rows.map(r => r.artistName),
      title: { text: null },
      labels: { formatter() { return this.value?.length > 18 ? this.value.slice(0, 18) + '…' : this.value; } }
    },
    yAxis: {
      min: 0,
      max: N,
      tickInterval: Math.max(1, Math.ceil(N / 10)),
      title: { text: 'Position (longer bar = better rank)' }
    },
    tooltip: {
      useHTML: true,
      formatter() {
        const r = rows[this.point.index];
        let html = `<b>${r.artistName}</b><br/>Position: #${r.position}<br/>Your score: ${r.rank}/100<br/>Spotify popularity: ${r.popularity}/100<br/>Followers: ${fmtFollowers(r.followerCount)}`;
        if (seriesData[this.point.index].url) html += `<br/><span style="color:#1DB954;">Click to view on Spotify</span>`;
        return html;
      }
    },
    plotOptions: {
      series: {
        dataLabels: {
          enabled: true,
          formatter() { return `#${rows[this.point.index].position}`; },
          style: {
            color: '#fff',      // white text
            textOutline: 'none' // remove shadow
          }
        },

        point: {
          events: {
            mouseOver() { if (this.options.url) document.body.style.cursor = 'pointer'; },
            mouseOut() { document.body.style.cursor = 'default'; },
            click() { if (this.options.url) window.open(this.options.url, '_blank', 'noopener'); }
          }
        }
      }
    },
    series: [{ name: 'Position', data: seriesData }],
    legend: { enabled: false },
    credits: { enabled: false }
  };

  return <HighchartsReact highcharts={Highcharts} options={options} />;
}

import React, { useMemo, useRef, useState, useCallback } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';

// --- Helpers ---
const MS_DAY = 24 * 3600 * 1000;
const MS_YEAR_APPROX = 365 * MS_DAY;
const MS_MONTH_APPROX = 30 * MS_DAY;

// Escape HTML safely
function escapeHTML(str) {
  if (typeof str !== 'string') return '';
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

// Convert "spotify:track:ID" → "https://open.spotify.com/track/ID"
function spotifyUriToUrl(uri) {
  if (!uri || typeof uri !== 'string' || !uri.startsWith('spotify:')) return null;
  return `https://open.spotify.com/${uri.replace('spotify:', '').replace(/:/g, '/')}`;
}

// Parse date safely and reject placeholders (e.g., year < 1900)
function toValidDateISO(iso) {
  const d = new Date(iso);
  if (!isFinite(d)) return null;
  const y = d.getUTCFullYear();
  return y >= 1900 ? d : null;
}

const yearStartUTC = (date) => Date.UTC(date.getUTCFullYear(), 0, 1);
const monthStartUTC = (date) => Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), 1);

// Group tracks by period key function
function groupByPeriod(tracks, keyFn) {
  const buckets = new Map();
  tracks.forEach(t => {
    const d = toValidDateISO(t.releaseDate);
    if (!d) return;
    const key = keyFn(d);
    if (!buckets.has(key)) buckets.set(key, { count: 0, tracks: [] });
    const b = buckets.get(key);
    b.count += 1;
    b.tracks.push({
      trackName: t.trackName,
      artistName: t.artistName,
      spotifyLink: t.spotifyLink
    });
  });
  const keys = [...buckets.keys()].sort((a, b) => a - b);
  const pts = keys.map(ts => ({
    x: ts,
    y: buckets.get(ts).count,
    custom: { tracks: buckets.get(ts).tracks }
  }));
  return { points: pts, minX: keys[0], maxX: keys[keys.length - 1] };
}

export default function ReleasesTimelineSmart({ data }) {
  const chartRef = useRef(null);

  // Precompute full YEAR view across all data
  const { yearPoints, allMinX, allMaxX } = useMemo(() => {
    const arr = Array.isArray(data) ? data : [];
    const grouped = groupByPeriod(arr, yearStartUTC);
    return {
      yearPoints: grouped.points,
      allMinX: grouped.minX,
      allMaxX: grouped.maxX
    };
  }, [data]);

  // State for current mode and view range
  const [mode, setMode] = useState('year'); // 'year' | 'month'
  const [viewRange, setViewRange] = useState(() => ({
    min: allMinX,
    max: allMaxX
  }));

  // Build MONTH series on demand for a given range
  const monthSeriesForRange = useCallback((min, max) => {
    const arr = Array.isArray(data) ? data : [];
    const filtered = arr.filter(t => {
      const d = toValidDateISO(t.releaseDate);
      if (!d) return false;
      const ts = d.getTime();
      return ts >= (min ?? -Infinity) && ts <= (max ?? Infinity);
    });
    const grouped = groupByPeriod(filtered, monthStartUTC);
    return grouped.points;
  }, [data]);

  // Decide mode based on range width
  const decideMode = useCallback((min, max) => {
    const width = (max ?? 0) - (min ?? 0);
    return width <= (MS_YEAR_APPROX + MS_MONTH_APPROX) ? 'month' : 'year';
  }, []);

  // Build the series to render based on mode + range
  const renderSeries = useMemo(() => {
    if (!allMinX || !allMaxX) {
      return { points: [], xMin: undefined, xMax: undefined };
    }
    if (mode === 'year') {
      const xMin = viewRange.min ?? allMinX;
      const xMax = viewRange.max ?? allMaxX;
      return { points: yearPoints, xMin, xMax };
    }
    const pts = monthSeriesForRange(viewRange.min ?? allMinX, viewRange.max ?? allMaxX);
    return { points: pts, xMin: viewRange.min, xMax: viewRange.max };
  }, [mode, viewRange, yearPoints, allMinX, allMaxX, monthSeriesForRange]);

  // Handle zoom/scroll via setExtremes
  const onSetExtremes = useCallback((e) => {
    const newMin = (typeof e.min === 'number') ? e.min : allMinX;
    const newMax = (typeof e.max === 'number') ? e.max : allMaxX;
    setViewRange({ min: newMin, max: newMax });
    setMode(decideMode(newMin, newMax));
  }, [allMinX, allMaxX, decideMode]);

  // Reset zoom button
  const resetZoom = () => {
    setViewRange({ min: allMinX, max: allMaxX });
    setMode('year');
    const chart = chartRef.current?.chart;
    if (chart?.xAxis?.[0]) {
      chart.xAxis[0].setExtremes(allMinX, allMaxX);
    }
  };

  // Tooltip shared for both modes, lists tracks in that period
  const tooltip = {
    useHTML: true,
    formatter() {
      const fmt = mode === 'year' ? '%Y' : '%b %Y';
      const title = Highcharts.dateFormat(fmt, this.x);
      const tracks = this.point.custom?.tracks || [];
      const total = tracks.length;

      const maxItems = 6;
      const items = tracks.slice(0, maxItems).map(t => {
        const url = spotifyUriToUrl(t.spotifyLink);
        const text = `${t.trackName} — ${t.artistName}`;
        return url
          ? `<li><a href="${url}" target="_blank" rel="noopener">${escapeHTML(text)}</a></li>`
          : `<li>${escapeHTML(text)}</li>`;
      });
      const more = total > maxItems ? `<li>…and ${total - maxItems} more</li>` : '';

      return `
        <div style="min-width:220px">
          <div style="font-weight:600;margin-bottom:4px">${escapeHTML(title)}</div>
          <div style="margin-bottom:6px"><b>${total}</b> release${total === 1 ? '' : 's'}</div>
          <ol style="margin:0;padding-left:16px">${items.join('')}${more}</ol>
        </div>
      `;
    }
  };

  const xAxis = {
    type: 'datetime',
    min: renderSeries.xMin,
    max: renderSeries.xMax,
    tickInterval: mode === 'year' ? (365 * MS_DAY) : (30 * MS_DAY),
    title: { text: null },
    events: { setExtremes: onSetExtremes }
  };

  const options = {
    chart: { type: 'areaspline', height: 380, backgroundColor: 'transparent', zoomType: 'x' },
    title: { text: mode === 'year' ? 'Releases Over Time (by year)' : 'Releases Over Time (by month)' },
    subtitle: {
      text:
        'Counts of tracks by their original release date. Drag to zoom for month-level detail.<br/>' +
        '<i>When were the tracks I’ve been listening to released?</i>',
      style: { color: '#A0A0A0', fontSize: '12px' },
      useHTML: true
    },
    xAxis,
    yAxis: {
      title: { text: 'Tracks released' },
      min: 0,
      allowDecimals: false
    },
    tooltip,
    plotOptions: {
      series: {
        marker: { enabled: true, radius: 3 },
        fillOpacity: 0.25
      },
      areaspline: { color: '#7CDB67' }
    },
    series: [{ name: 'Releases', data: renderSeries.points }],
    legend: { enabled: false },
    credits: { enabled: false }
  };

  return (
    <div>
      <div style={{ display: 'flex', gap: 8, alignItems: 'center', margin: '4px 0 8px' }}>
        <button
          onClick={resetZoom}
          style={{ padding: '4px 8px', borderRadius: 6, border: '1px solid #333', background: '#191414', color: '#ddd' }}
        >
          Reset zoom
        </button>
        <span style={{ color: '#aaa', fontSize: 12 }}>
          View: <b>{mode === 'year' ? 'Year' : 'Month'}</b>
        </span>
      </div>
      <HighchartsReact ref={chartRef} highcharts={Highcharts} options={options} />
    </div>
  );
}

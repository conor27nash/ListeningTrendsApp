import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';
import { getArtistColour } from '../utils/artistColours';

// Convert "spotify:artist:ID" → "https://open.spotify.com/artist/ID"
function spotifyUriToUrl(uri) {
    if (!uri || typeof uri !== 'string' || !uri.startsWith('spotify:')) return null;
    return `https://open.spotify.com/${uri.replace('spotify:', '').replace(/:/g, '/')}`;
}

// Marker shape by delta
function shapeByDelta(delta) {
    if (delta >= 10) return 'triangle';
    if (delta <= -10) return 'triangle-down';
    return 'circle';
}

export default function RankVsPopularityScatter({ data, top = 40 }) {
    const { points, minX, maxX, minY, maxY } = useMemo(() => {
        const arr = Array.isArray(data) ? [...data] : [];
        const topRows = arr.sort((a, b) => (b.rank ?? 0) - (a.rank ?? 0)).slice(0, top);

        let minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;

        const pts = topRows.map(d => {
            const yours = d.rank ?? 0;
            const pop = d.popularity ?? 0;
            const delta = yours - pop;

            minX = Math.min(minX, pop);
            maxX = Math.max(maxX, pop);
            minY = Math.min(minY, yours);
            maxY = Math.max(maxY, yours);

            return {
                name: d.artistName,
                spotifyLink: d.spotifyLink,
                x: pop,
                y: yours,
                marker: {
                    symbol: shapeByDelta(delta),
                    radius: 6,
                    fillColor: getArtistColour(d.artistName),
                    lineColor: '#222',
                    lineWidth: 1
                },
                custom: { ...d, delta }
            };
        });

        const pad = 2;
        return {
            points: pts,
            minX: Math.max(0, minX - pad),
            maxX: Math.min(100, maxX + pad),
            minY: Math.max(0, minY - pad),
            maxY: Math.min(100, maxY + pad)
        };
    }, [data, top]);

    const options = {
        chart: { type: 'scatter', zoomType: 'xy', height: 520, backgroundColor: 'transparent' },
        title: { text: 'Your Score vs Spotify Popularity' },
        subtitle: {
            text: 'Above the diagonal: you like them more than the crowd. Below: the crowd likes them more.<br/><i>Do you like artists more or less than the global average?</i>',
            style: { color: '#A0A0A0', fontSize: '12px' },
            useHTML: true
        },
        xAxis: {
            title: { text: 'Spotify popularity (0–100)' },
            min: minX,
            max: maxX,
            tickInterval: 10
        },
        yAxis: {
            title: { text: 'Your score (0–100)' },
            min: minY,
            max: maxY,
            tickInterval: 10
        },
        tooltip: {
            useHTML: true,
            formatter() {
                const c = this.point.custom;
                const sign = c.delta > 0 ? '+' : '';
                const clickHint = this.point.spotifyLink
                    ? `<br/><span style="color:#1DB954;">Click to view on Spotify</span>`
                    : '';
                return `<b>${c.artistName}</b><br/>
                        Your score: <b>${c.rank}</b><br/>
                        Spotify popularity: <b>${c.popularity}</b><br/>
                        Δ (you − Spotify): <b>${sign}${c.delta}</b><br/>
                        Followers: ${c.followerCount?.toLocaleString?.()}
                        ${clickHint}`;
            }
        },
        plotOptions: {
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click() {
                            const url = spotifyUriToUrl(this.spotifyLink);
                            if (url) window.open(url, '_blank', 'noopener');
                        }
                    }
                }
            }
        },
        series: [
            {
                type: 'line',
                name: 'y = x',
                data: [[0, 0], [100, 100]],
                enableMouseTracking: false,
                color: '#555',
                dashStyle: 'ShortDot',
                marker: { enabled: false },
                states: { hover: { enabled: false }, inactive: { opacity: 1 } }
            },
            {
                name: 'Artists',
                data: points
            }
        ],
        legend: { enabled: false },
        credits: { enabled: false }
    };

    return <HighchartsReact highcharts={Highcharts} options={options} />;
}

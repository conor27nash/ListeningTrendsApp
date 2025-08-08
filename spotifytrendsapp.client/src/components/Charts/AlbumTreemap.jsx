import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';
import { getArtistColour } from '../utils/artistColours';

// Convert "spotify:album:ID" → "https://open.spotify.com/album/ID"
function spotifyUriToUrl(uri) {
    if (!uri || typeof uri !== 'string' || !uri.startsWith('spotify:')) return null;
    return `https://open.spotify.com/${uri.replace('spotify:', '').replace(/:/g, '/')}`;
}

// Lighten an HSL colour
function lightenColour(hsl, percent = 20) {
    return hsl.replace(/(\d+)%\)$/, (_, l) => `${Math.min(100, parseInt(l, 10) + percent)}%)`);
}

export default function AlbumTreemap({
    data = [],
    height = 520,
    title = 'Album Mosaic'
}) {
    const points = useMemo(() => {
        if (!Array.isArray(data)) return [];

        const grouped = {};
        data.forEach((d) => {
            const artist = d.artistName || 'Unknown Artist';
            if (!grouped[artist]) {
                grouped[artist] = { totalCount: 0, albums: [] };
            }
            grouped[artist].totalCount += d.count ?? 0;
            grouped[artist].albums.push(d);
        });

        let parentId = 0;
        const treemapData = [];

        Object.entries(grouped).forEach(([artist, info]) => {
            const parentColor = getArtistColour(artist);
            const parentNodeId = `artist-${parentId++}`;

            treemapData.push({
                id: parentNodeId,
                name: `${artist} (${info.totalCount})`, // show artist name + total count
                value: info.totalCount,
                color: parentColor
            });

            // Sort albums by plays
            info.albums.sort((a, b) => (b.count ?? 0) - (a.count ?? 0));

            info.albums.forEach((album, i) => {
                treemapData.push({
                    id: `${parentNodeId}-album-${i}`,
                    name: album.albumName,
                    value: album.count ?? 0,
                    parent: parentNodeId,
                    color: lightenColour(parentColor, 15),
                    spotifyLink: album.spotifyLink,
                    custom: { artistName: artist, count: album.count ?? 0 }
                });
            });
        });

        return treemapData;
    }, [data]);

    const options = {
        chart: { type: 'treemap', backgroundColor: 'transparent', height },
        title: { text: title },
        subtitle: {
            text: 'Tile size = how many tracks you played from each album, grouped by artist.<br/><i>Which albums and artists dominate my listening?</i>',
            style: { color: '#A0A0A0', fontSize: '12px' },
            useHTML: true
        },
        tooltip: {
            useHTML: true,
            formatter() {
                if (!this.point.parent) {
                    // artist tile
                    return `<b>${this.point.name}</b>`;
                }
                const a = this.point.custom?.artistName ?? '';
                const c = this.point.custom?.count ?? this.point.value ?? 0;
                const clickHint = this.point.spotifyLink
                    ? `<br/><span style="color:#1DB954;">Click to view on Spotify</span>`
                    : '';
                return `<b>${this.point.name}</b><br/>${a}<br/>Plays: <b>${c}</b>${clickHint}`;
            }
        },
        plotOptions: {
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click() {
                            if (this.spotifyLink) {
                                const url = spotifyUriToUrl(this.spotifyLink);
                                if (url) window.open(url, '_blank', 'noopener');
                            }
                        }
                    }
                }
            },
            treemap: {
                layoutAlgorithm: 'squarified',
                borderColor: '#111',
                borderWidth: 1,
                levels: [
                    {
                        level: 1, // Artist level
                        dataLabels: {
                            enabled: true,
                            style: { textOutline: 'none', color: '#333', fontWeight: 600, fontSize: '13px', backgroundColor: 'rgba(255, 255, 255, 0.8)' }
                        }
                    },
                    {
                        level: 2, // Album level
                        dataLabels: {
                            enabled: true,
                            style: { textOutline: 'none', color: '#555', fontWeight: 600, fontSize: '11px' },
                            formatter() {
                                const n = this.point.name.length > 22 ? this.point.name.slice(0, 22) + '…' : this.point.name;
                                const a = this.point.custom?.artistName ?? '';
                                const c = this.point.custom?.count ?? '';
                                return `${n}<br/><span style="opacity:.8">${a}</span><br/><span style="opacity:.7">${c}</span>`;
                            }
                        }
                    }
                ]
            }
        },
        series: [
            {
                type: 'treemap',
                layoutAlgorithm: 'squarified',
                data: points
            }
        ],
        legend: { enabled: false },
        credits: { enabled: false }
    };

    return <HighchartsReact highcharts={Highcharts} options={options} />;
}

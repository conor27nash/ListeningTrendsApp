import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';
import { getArtistColour } from '../utils/artistColours';

const fmtFollowers = (n = 0) =>
    n >= 1e6 ? (n / 1e6).toFixed(1) + 'M' :
    n >= 1e3 ? (n / 1e3).toFixed(1) + 'k' :
    String(n);

function spotifyUriToUrl(uri) {
    if (!uri || typeof uri !== 'string' || !uri.startsWith('spotify:')) return null;
    return `https://open.spotify.com/${uri.replace('spotify:', '').replace(/:/g, '/')}`;
}

export default function PopularityFollowersScatter({ data }) {
    if (!Array.isArray(data) || data.length === 0) {
        return null;
    }

    const points = useMemo(() => {
        const followerCounts = data.map(d => d.followerCount ?? 1);
        const minFollowers = Math.min(...followerCounts);
        const maxFollowers = Math.max(...followerCounts);

        return data.map(d => {
            const popularity = d.popularity ?? 0;
            const followers = d.followerCount ?? 1;
            const normFollowers = maxFollowers > minFollowers
                ? ((followers - minFollowers) / (maxFollowers - minFollowers)) * 100
                : 0;
            const combinedScore = (popularity + normFollowers) / 2;

            return {
                name: d.artistName,
                spotifyLink: d.spotifyLink,
                x: popularity,
                y: followers,
                z: combinedScore,
                marker: {
                    fillColor: getArtistColour(d.artistName),
                    lineColor: '#222',
                    lineWidth: 1
                },
                custom: { popularity, followers, combinedScore }
            };
        });
    }, [data]);

    const minPopularity = Math.max(
        0,
        Math.floor(Math.min(...points.map(p => p.x)) / 5) * 5
    );

    const minFollowersForAxis = (() => {
        const minVal = Math.min(...points.map(p => p.y));
        const exp = Math.floor(Math.log10(minVal));
        return Math.pow(10, exp);
    })();

    const options = {
        chart: { type: 'bubble', zoomType: 'xy', height: 520, backgroundColor: 'transparent' },
        title: { text: 'Spotify Popularity vs Followers' },
        subtitle: {
            text: 'Bubble size = combination of popularity and follower count.<br/><i>Which artists are globally popular vs which have huge followings?</i>',
            style: { color: '#A0A0A0', fontSize: '12px' },
            useHTML: true
        },
        xAxis: {
            title: { text: 'Spotify Popularity (0–100)' },
            min: minPopularity,
            max: 100,
            tickInterval: 10
        },
        yAxis: {
            title: { text: 'Followers' },
            type: 'logarithmic',
            min: minFollowersForAxis,
            minorTickInterval: null,
            labels: {
                formatter() {
                    const v = this.value;
                    if (v >= 1e6) return v / 1e6 + 'M';
                    if (v >= 1e3) return v / 1e3 + 'k';
                    return v;
                }
            }
        },
        tooltip: {
            useHTML: true,
            formatter() {
                const c = this.point.custom;
                const clickHint = this.point.spotifyLink
                    ? `<br/><span style="color:#1DB954;">Click to view on Spotify</span>`
                    : '';
                return `<b>${this.point.name}</b><br/>
                        Popularity: ${c.popularity}/100<br/>
                        Followers: ${fmtFollowers(c.followers)}<br/>
                        Combined score: ${c.combinedScore.toFixed(1)}/100
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
        series: [{
            name: 'Artists',
            data: points
        }],
        legend: { enabled: false },
        credits: { enabled: false }
    };

    return <HighchartsReact highcharts={Highcharts} options={options} />;
}

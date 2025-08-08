import React, { useState, useEffect } from 'react';
import TimeRangeToggle from '../TimeRangeToggle/TimeRangeToggle';
import { getAnalytics } from '../../api/analytics';

import AlbumTreemap from '../Charts/AlbumTreemap';
import ArtistLeaderboardRanks from '../Charts/ArtistLeaderboardBar';
import RankVsPopularityScatter from '../Charts/RankVsPopularityScatter';
import PopularityFollowersScatter from '../Charts/PopularityFollowersScatter';
import TrackYearColumn from '../Charts/TrackYearColumn';
import ReleasesTimelineSmart from '../Charts/ReleasesTimelineArea';
import GenreBubble from '../Charts/GenreBubble';
import TopGenresDonut from '../Charts/TopGenresDonut';

import './Dashboard.css';

export default function Dashboard() {
    const [timeRange, setTimeRange] = useState('medium_term');
    const [analytics, setAnalytics] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    useEffect(() => {
        setLoading(true);
        setError(null);
        getAnalytics(timeRange)
            .then(data => setAnalytics(data))
            .catch(err => setError(err.message))
            .finally(() => setLoading(false));
    }, [timeRange]);

    return (
        <div className="page">
            <div className="container">
                <div className="header">
                    <h2>Spotify Trends</h2>
                    <TimeRangeToggle current={timeRange} onChange={setTimeRange} />
                </div>

                    {loading && (
                        <div className="dashboard-loading">
                            <div className="spinner"></div>
                        </div>
                    )}
                {error && <p className="error">Error: {error}</p>}

                {analytics && (
                    <>
                        {/* Artist Insights Section */}
                        <section className="section-container">
                            <h3>Artist Insights</h3>
                            <p>How do my favorite artists rank compared to global popularity and followers?</p>
                            <div className="grid">
                                <div className="card span-6">
                                    <ArtistLeaderboardRanks data={analytics.artistLeaderboardData} />
                                </div>
                                <div className="card span-6">
                                    <PopularityFollowersScatter data={analytics.artistLeaderboardData} />
                                </div>
                                <div className="card span-6">
                                    <RankVsPopularityScatter data={analytics.artistLeaderboardData} top={40} />
                                </div>
                            </div>
                        </section>

                        {/* Listening Timeline Section */}
                        <section className="section-container">
                            <h3>Listening Timeline</h3>
                            <p>How much of my listening is from older vs newer music?</p>
                            <div className="grid">
                                <div className="card span-6">
                                    <TrackYearColumn data={analytics.trackTimelineData} />
                                </div>
                                <div className="card span-6">
                                    <ReleasesTimelineSmart data={analytics.trackTimelineData} />
                                </div>
                            </div>
                        </section>

                        {/* Genre Exploration Section */}
                        <section className="section-container">
                            <h3>Genre Exploration</h3>
                            <p>Which genres dominate my listening and how do they compare?</p>
                            <div className="grid">
                                <div className="card span-6">
                                    <GenreBubble data={analytics.genreBubbleData} />
                                </div>
                                <div className="card span-6">
                                    <TopGenresDonut data={analytics.genreBubbleData} />
                                </div>
                            </div>
                        </section>

                        {/* Album Mosaic Section */}
                        <section className="section-container">
                            <h3>Album Mosaic</h3>
                            <p>A visual overview of albums I listen to most.</p>
                            <div className="card">
                                <AlbumTreemap data={analytics.albumMosaicData} height={520} />
                            </div>
                        </section>
                    </>
                )}
            </div>
        </div>
    );
}

import React, { useMemo } from 'react';
import Highcharts from '../../setupHighcharts';
import HighchartsReact from 'highcharts-react-official';

const toYear = (s) => {
    const y = new Date(s).getUTCFullYear();
    return isNaN(y) ? null : y;
};

export default function TrackYearColumn({ data }) {
    const { cats, vals } = useMemo(() => {
        const map = new Map();
        (data || []).forEach(t => {
            const y = toYear(t.releaseDate);
            if (!y || y < 1900) return;
            map.set(y, (map.get(y) || 0) + 1);
        });
        const years = [...map.keys()].sort((a, b) => a - b);
        return { cats: years, vals: years.map(y => map.get(y)) };
    }, [data]);

    const options = {
        chart: { type: 'column', height: 380, backgroundColor: 'transparent' },
        title: { text: 'Tracks by Release Year' },
        subtitle: {
            text: 'Number of tracks in your listening history by release year.<br/><i>How much of my listening is from older vs newer music?</i>',
            style: { color: '#A0A0A0', fontSize: '12px' },
            useHTML: true
        },
        xAxis: { categories: cats.map(String) },
        yAxis: { title: { text: 'Tracks' }, min: 0, allowDecimals: false },
        tooltip: { pointFormat: '<b>{point.y} tracks</b>' },
        plotOptions: { column: { borderRadius: 3 } },
        series: [{ name: 'Tracks', data: vals, color: '#1DB954' }],
        legend: { enabled: false },
        credits: { enabled: false }
    };

    return <HighchartsReact highcharts={Highcharts} options={options} />;
}

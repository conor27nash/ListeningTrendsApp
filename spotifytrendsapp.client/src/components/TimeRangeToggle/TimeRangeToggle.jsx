import React from 'react';
import './TimeRangeToggle.css';

const TimeRangeToggle = ({ current = "short_term", onChange }) => (
  <div className="time-toggle">
    {['short_term', 'medium_term', 'long_term'].map(term => {
      const label = term.split('_')[0];
      return (
        <button
          key={term}
          className={term === current ? 'active' : ''}
          onClick={() => onChange(term)}
        >
          {label.charAt(0).toUpperCase() + label.slice(1)} Term
        </button>
      );
    })}
  </div>
);

export default TimeRangeToggle;

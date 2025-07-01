import React from 'react';
import './TimeRangeToggle.css';

const TimeRangeToggle = ({ current, onChange }) => (
  <div className="time-toggle">
    {['short_term', 'medium_term', 'long_term'].map(term => (
      <button
        key={term}
        className={term === current ? 'active' : ''}
        onClick={() => onChange(term)}
      >
        {term.charAt(0).toUpperCase() + term.slice(1)} Term
      </button>
    ))}
  </div>
);

export default TimeRangeToggle;

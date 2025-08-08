import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'react-router-dom';

import './Sidebar.css';
import Login from '../../Login/Login';

const Sidebar = () => {
    const [isOpen, setIsOpen] = useState(false);

    const toggleSidebar = () => {
        setIsOpen(!isOpen);
    };

    return (
        <>
            <div className="sidebar-toggle" onClick={toggleSidebar}>
                ☰
            </div>
            <div className={`sidebar ${isOpen ? 'open' : ''}`}>
                <Login />
                <ul>
                    <li><NavLink to="/" end className={({ isActive }) => isActive ? 'active' : ''}>Home</NavLink></li>
                    <li><NavLink to="/top-tracks" className={({ isActive }) => isActive ? 'active' : ''}>Top Tracks</NavLink></li>
                    <li><NavLink to="/top-artists" className={({ isActive }) => isActive ? 'active' : ''}>Top Artists</NavLink></li>
                    <li><NavLink to="/recent-played" className={({ isActive }) => isActive ? 'active' : ''}>Recently Played</NavLink></li>
                </ul>
            </div>
        </>
    );
};

export default Sidebar;

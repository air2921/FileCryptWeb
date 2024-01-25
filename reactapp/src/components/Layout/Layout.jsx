import React from 'react';
import { Link, Outlet } from 'react-router-dom';

function Layout() {
    return (
        <>
            <header>
                <Link to="/">Home</Link>
                <Link to="/about">About</Link>
                <Link to="/policy">Policy</Link>
            </header>

            <Outlet />
        </>
    );
}

export default Layout;
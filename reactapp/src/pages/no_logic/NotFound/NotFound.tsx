import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import './NotFound.css'

const NotFound = () => {
    return (
        <div className="error-container">
            <div className="error-code">
                <h1>404</h1>
            </div>
            <br />
            <div className="error-description">
                <h2>Page not found</h2>
            </div>
            <div className="divider"></div>
            <Link to="/" className="main-button">
                Back to main page
            </Link>
        </div>
    );
}

export default NotFound;
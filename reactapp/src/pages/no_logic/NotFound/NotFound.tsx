import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import './NotFound.css'

const NotFound = () => {
    return (
        <div className="error-container">
            <div className="error-code">
                <h1>404</h1>
            </div>
            <div className="error-description">
                <h2>Not Found</h2>
            </div>
            <div className="divider"></div>
            <Link to="/" className="main-button">
                Back to main page
            </Link>
        </div>
    );
}

export default NotFound;
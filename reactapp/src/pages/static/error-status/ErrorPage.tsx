import React from 'react';
import { Link } from 'react-router-dom';
import './ErrorPage.css'

const ErrorPage: React.FC<{ statusCode: number; message: string }> = ({ statusCode, message }) => {
    return (
        <div className="error-container">
            <div className="error-code">
                <h1>{statusCode}</h1>
            </div>
            <div className="error-description">
                <h2>{message}</h2>
            </div>
            <div className="divider"></div>
            <Link to="/" className="main-button">
                Back to main page
            </Link>
        </div>
    );
}

export default ErrorPage;
import React from 'react';

function Error({ errorMessage, errorFont }: ErrorProps) {
    return (
        <div className="error-container">
            <i className="material-icons-sharp">{errorFont}</i>
            <div>{errorMessage}</div>
        </div>
    );
}

export default Error;
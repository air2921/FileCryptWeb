import React from 'react';

function Message({ message, font }: MessageProps) {
    return (
        <div className="error-container">
            <i className="material-icons-sharp">{font}</i>
            <div>{message}</div>
        </div>
    );
}

export default Message;
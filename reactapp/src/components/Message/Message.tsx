import React from 'react';
import Font from '../Font/Font';

function Message({ message, font }: MessageProps) {
    return (
        <div className="error-container">
            <Font font={font} />
            <div>{message}</div>
        </div>
    );
}

export default Message;
import React from 'react';
import Font from '../icon/Font';

function Message({ message, font }: MessageProps) {
    return (
        <div className="message-container">
            <Font font={font} />
            <div>{message}</div>
        </div>
    );
}

export default Message;
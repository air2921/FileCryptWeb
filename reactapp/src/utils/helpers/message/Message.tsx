import React from 'react';

function Message({ message, icon }: { message: string, icon: string }) {
    return (
        <div className="message-container">
            <div>{message}</div>
        </div>
    );
}

export default Message;
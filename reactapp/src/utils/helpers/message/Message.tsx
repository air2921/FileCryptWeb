import React from 'react';
import Icon from '../icon/Icon';

function Message({ message, icon }: { message: string, icon: string }) {
    return (
        <div className="message-container">
            <Icon icon={icon} />
            <div>{message}</div>
        </div>
    );
}

export default Message;
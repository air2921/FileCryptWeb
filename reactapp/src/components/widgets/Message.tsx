import React, { useEffect, useState } from 'react';
import Icon from './icon/Icon';

interface MessageProps {
    message: string;
    success: boolean;
    onMessageChange: (message: string) => void;
}

function MessageP(props: MessageProps) {
    const [icon, setIcon] = useState('');

    useEffect(() => {
        if (props.success) {
            setIcon('success');
        } else {
            setIcon('error');
        }

        const timer = setTimeout(() => {
            setIcon('');
            props.onMessageChange('');
        }, 5000);

        return () => clearTimeout(timer);
    }, [props.success, props.message, props.onMessageChange]);

    return (
        <>
            {props.message && icon && (
                <div className="message-container">
                    <div>{props.message}</div>
                    <Icon icon={icon} height={24} width={24} />
                </div>
            )}
        </>
    );
}

function Message({ message, icon }: { message: string, icon: string }) {
    return (
        <div className="message-container">
            <div>{message}</div>
            <Icon icon={icon} height={24} width={24} />
        </div>
    );
}

export default Message;
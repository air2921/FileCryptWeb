import React, { useEffect, useState } from 'react';
import NotificationList from '../../../components/List/Notifications/NotificationList';
import AxiosRequest from '../../../api/AxiosRequest';
import Font from '../../../components/Font/Font';
import Message from '../../../components/Message/Message';

const Notifications = () => {
    const [errorMessage, setErrorMessage] = useState('');
    const [notificationList, setNotifications] = useState(null);
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: `api/core/notifications/all?skip=${skip}&count=${step}`, method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setNotifications(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

    const handleLoadMore = () => {
        setSkip(prevSkip => prevSkip + step);
    };

    const handleBack = () => {
        setSkip(prevSkip => Math.max(0, prevSkip - step));
    };

    const deleteNotification = async (notificationId: number) => {
        const response = await AxiosRequest({ endpoint: `api/core/notifications/${notificationId}`, method: 'DELETE', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setLastTimeModified(Date.now());
        }
        else {
            setMessage(response.data);
            setFont('error');
        }

        setTimeout(() => {
            setMessage('');
            setFont('');
        }, 5000)
    }

    useEffect(() => {
        fetchData();
    }, [lastTimeModified, skip]);

    if (!notificationList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { notifications } = notificationList as { notifications: any[] }

    return (
        <div className="container">
            {message && font && < Message message={message} font={font} />}
            <NotificationList notifications={notifications} deleteNotification={deleteNotification} />
            {skip > 0 && <button onClick={handleBack}><Font font={'arrow_back'} /></button>}
            {notifications.length > step - 1 && <button onClick={handleLoadMore}><Font font={'arrow_forward'} /></button>}
        </div>
    );
}

export default Notifications;
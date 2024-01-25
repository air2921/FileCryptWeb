import React, { useEffect, useState } from 'react';
import NotificationList from '../../../components/Notifications/NotificationList';
import AxiosRequest from '../../../api/AxiosRequest';
import Button from '../../../components/Helpers/Button';
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
            <NotificationList notifications={notifications} deleteNotification={deleteNotification} />
            {message && font && < Message message={message} font={font} />}
            {skip > 0 && <Button onClick={handleBack}><Font font={'arrow_back'} /></Button>}
            {notifications.length > step - 1 && <Button onClick={handleLoadMore}><Font font={'arrow_forward'} /></Button>}
        </div>
    );
}

export default Notifications;
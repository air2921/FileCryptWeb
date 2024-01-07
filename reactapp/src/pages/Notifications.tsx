import React, { useEffect, useState } from 'react';
import NotificationList from '../components/Notifications/NotificationList';
import AxiosRequest from '../api/AxiosRequest';
import Button from '../components/Helpers/Button';

const Notifications = () => {
    const [errorMessage, setErrorMessage] = useState('');
    const [notificationList, setNotifications] = useState(null);

    const [skip, setSkip] = useState(0);
    const step = 10;
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [deletingError, setDeletingError] = useState('');

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
            setDeletingError(response.data);
        }
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
            <NotificationList notifications={notifications} deleteNotification={deleteNotification} error={deletingError} />
            {skip > 0 && <Button onClick={handleBack}>Back</Button>}
            {notifications.length > step - 1 && <Button onClick={handleLoadMore}>Load More</Button>}
        </div>
    );
}

export default Notifications;
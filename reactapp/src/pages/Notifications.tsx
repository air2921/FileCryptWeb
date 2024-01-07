import React, { useEffect, useState } from 'react';
import NotificationList from '../components/Notifications/NotificationList';
import AxiosRequest from '../api/AxiosRequest';

const Notifications = () => {
    const [errorMessage, setErrorMessage] = useState('');
    const [notificationList, setNotifications] = useState(null);

    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [deletingError, setDeletingError] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({ endpoint: 'api/core/notifications/all', method: 'GET', withCookie: true, requestBody: null });

        if (response.isSuccess) {
            setNotifications(response.data);
        }
        else {
            setErrorMessage(response.data);
        }
    }

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
    }, [lastTimeModified]);

    if (!notificationList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { notifications } = notificationList as { notifications: any[] }

    return (
        <div className="container">
            <NotificationList notifications={notifications} deleteNotification={deleteNotification} error={deletingError} />
        </div>
    );
}

export default Notifications;
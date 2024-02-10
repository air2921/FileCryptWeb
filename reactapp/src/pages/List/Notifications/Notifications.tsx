import React, { useEffect, useState } from 'react';
import NotificationList from '../../../components/List/Notifications/NotificationList';
import AxiosRequest from '../../../api/AxiosRequest';
import Message from '../../../components/Message/Message';

const Notifications = () => {
    const [skip, setSkip] = useState(0);
    const step = 10;
    const [orderBy, setOrderBy] = useState('true');
    const [priority, setPriority] = useState('');
    const [isChecked, setChecked] = useState('');

    const [errorMessage, setErrorMessage] = useState('');
    const [notificationList, setNotifications] = useState(null);
    const [lastTimeModified, setLastTimeModified] = useState(Date.now());
    const [message, setMessage] = useState('');
    const [font, setFont] = useState('');

    const fetchData = async () => {
        const response = await AxiosRequest({
            endpoint: `api/core/notifications/all?skip=${skip}&count=${step}&byDesc=${orderBy}&priority=${priority}&isChecked=${isChecked}`,
            method: 'GET',
            withCookie: true,
            requestBody: null
        });

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
    }, [lastTimeModified, skip, orderBy, priority, isChecked]);

    if (!notificationList) {
        return <div className="error">{errorMessage || 'Loading...'}</div>;
    }

    const { notifications } = notificationList as { notifications: any[] }

    const SortNotification = () => {
        return (
            <div className="sort">
                <details>
                    <summary>
                        <span>Priority</span>
                    </summary>
                    <select
                        className="priority"
                        id="priority"
                        required={true}
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}>
                        <option value="">All</option>
                        <option value="Trade">Trade</option>
                        <option value="Info">Info</option>
                        <option value="Warning">Warning</option>
                        <option value="Security">Security</option>
                    </select>
                </details>
                <details>
                    <summary>
                        <span>Is checked</span>
                    </summary>
                    <select
                        className="checked"
                        id="checked"
                        required={true}
                        value={isChecked}
                        onChange={(e) => setChecked(e.target.value)}>
                        <option value="">All</option>
                        <option value="true">Only checked</option>
                        <option value="false">Only non-checked</option>
                    </select>
                </details>
                <details>
                    <summary>
                        <span>Order by</span>
                    </summary>
                    <select
                        className="order-by"
                        id="order"
                        required={true}
                        value={orderBy}
                        onChange={(e) => setOrderBy(e.target.value)}>
                        <option value="true">Order by descending</option>
                        <option value="false">Order by ascending</option>
                    </select>
                </details>
            </div>
        );
    }

    return (
        <div className="container">
            <div className="notifications">
                <SortNotification />
                <NotificationList notifications={notifications} deleteNotification={deleteNotification} />
                <div className="scroll">
                    {skip > 0 && <button onClick={handleBack}>Previous</button>}
                    {notifications.length > step - 1 && <button onClick={handleLoadMore}>Next</button>}
                </div>
            </div>
            <div className="message">
                {message && font && < Message message={message} font={font} />}
            </div>
        </div>
    );
}

export default Notifications;
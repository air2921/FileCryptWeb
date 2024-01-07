import React from 'react';
import DateComponent from '../Date/Date';
import Button from '../Helpers/Button';
import Message from '../Message/Message';
import Font from '../Font/Font';

function NotificationList({ notifications, deleteNotification, error }: NotificationListProps) {

    if (!notifications || notifications.every(notification => notification === null)) {
        return <div><Message message={'No received notifications'} font='storage' /></div>;
    }

    return (
        <ul>
            <Message message={'Your Notifications'} font='storage' />
            {notifications
                .filter(notification => notification !== null)
                .map(notification => (
                    <li key={notification.notification_id} className="notification">
                        <div className="notification-header">
                            <div className="notification-icon">
                                {
                                    notification.priority === 'Trade' ? <i className="material-icons-sharp">key</i> :
                                    notification.priority === 'Info' ? <i className="material-icons-sharp">info</i> :
                                    notification.priority === 'Warning' ? <i className="material-icons-sharp">warning</i> :
                                    notification.priority === 'Security' ? <i className="material-icons-sharp">security</i> :
                                    null
                                }
                            </div>
                            <div className="priority">{notification.priority}</div>
                            <div className="header">{notification.message_header}</div>
                        </div>
                        <div className="notification-details">
                            <div className="info">
                                <div className="body">{notification.message}</div>
                                <div className="id">
                                    <div className="notification-id">NID#{notification.notification_id}</div>
                                    <div className="user-id">UID#{notification.receiver_id}</div>
                                </div>
                            </div>
                            <div className="time"><DateComponent date={notification.send_time} /></div>
                        </div>
                        {notification.priority == 'Info' && deleteNotification && (
                            <Button onClick={() => deleteNotification(notification.notification_id)}>
                                <Font font={'delete'} />
                            </Button>
                        )}
                        {error && <Message message={error} font={'error'} />}
                    </li>
                ))}
        </ul>
    );
}

export default NotificationList;
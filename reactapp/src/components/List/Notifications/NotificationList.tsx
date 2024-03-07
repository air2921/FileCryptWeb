import React from 'react';
import Message from '../../../utils/helpers/message/Message';
import DateComponent from '../../../utils/helpers/date/Date';
import Font from '../../../utils/helpers/icon/Font';

function NotificationList({ notifications, deleteNotification }: NotificationListProps) {

    if (!notifications || notifications.every(notification => notification === null)) {
        return <div><Message message={'No received notifications'} font='storage' /></div>;
    }

    let font;

    if (notifications.some(notification => notification !== null && !notification.is_checked)) {
        font = 'notifications_active';
    }
    else {
        font = 'notifications';
    }

    return (
        <ul>
            <Message message={'Your Notifications'} font={font} />
            {notifications
                .filter(notification => notification !== null)
                .map(notification => (
                    <li key={notification.notification_id} className="notification">
                        <div className="notification-header">
                            <div className="notification-icon">
                                {
                                    notification.priority === 'Trade' ? <Font font={'key'} /> :
                                    notification.priority === 'Info' ? <Font font={'info'} /> :
                                    notification.priority === 'Warning' ? <Font font={'warning'} /> :
                                    notification.priority === 'Security' ? <Font font={'security'} /> :
                                    <Font font={'notifications'} />
                                }
                            </div>
                            <div className="priority">{notification.priority}</div>
                            <div className="header">{notification.message_header}</div>
                        </div>
                        <div className="notification-details">
                            <div className="info">
                                <div className="body">{notification.message}</div>
                                <div className="id">
                                    <div className="notification-id">Notification ID#{notification.notification_id}</div>
                                    <div className="user-id">User ID#{notification.receiver_id}</div>
                                </div>
                            </div>
                            <div className="time"><DateComponent date={notification.send_time} /></div>
                        </div>
                        {notification.priority == 'Info' && deleteNotification && (
                            <button onClick={() => deleteNotification(notification.notification_id)}>
                                <Font font={'delete'} />
                            </button>
                        )}
                    </li>
                ))}
        </ul>
    );
}

export default NotificationList;
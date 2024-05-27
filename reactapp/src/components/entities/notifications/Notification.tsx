import React from "react";
import { NotificationProps } from "../../../utils/api/Notifications";
import Icon from "../../widgets/icon/Icon";
import { dateFormate, lineFormate } from "../../../utils/helpers/Formatter";

function Notification({ notification, isShowBody }: { notification: NotificationProps, isShowBody: boolean }) {
    function setNotificationType(type: number): string {
        switch (type) {
            case 101:
                return 'key';
            case 102:
                return 'info';
            case 301:
                return 'warning';
            case 302:
                return 'danger';
            default:
                return 'notification';
        }
    }

    return (
        <div className="entity-notification-container">
            <div className={notification.is_checked ? "checked" : "none-checked"}>
                <div><Icon icon={setNotificationType(notification.priority)} height={24} width={24} /></div>
                <div>{notification.message_header}</div>
                <div>{dateFormate(notification.send_time)}</div>
                {isShowBody && (
                    <div>
                        {lineFormate(notification.message)}
                    </div>
                )}
            </div>
        </div>
    );
}

export default Notification;
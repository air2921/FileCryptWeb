import axios from "axios";
import { BASE_URL, errorHandler } from './Helper';

export const SPLITTER = '|new_line|';

export interface NotificationSortProps {
    skip: number;
    count: number;
    orderByDesc: string;
    priority?: string;
    isChecked?: string;
}

export interface NotificationProps{
    notification_id: number;
    message_header: string;
    message: string;
    priority: string;
    send_time: string;
    is_checked: boolean;
    user_id: number;
}

export async function getNotification(notificationId: number) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/notification/${notificationId}`,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: response.data
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function getNotifications(p: NotificationSortProps) {
    try {
        const response = await axios.get(
            BASE_URL + `api/core/notification/range/?skip=${p.skip}&count=${p.count}&byDesc=${p.orderByDesc}&priority=${p.priority}&isChecked=${p.isChecked}`,
            { withCredentials: true }
        );

        return {
            success: true,
            statusCode: response.status,
            data: response.data
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}

export async function deleteNotification(notificationId: number) {
    try {
        const response = await axios.delete(BASE_URL + `api/core/notification/${notificationId}`, { withCredentials: true });

        return {
            success: true,
            statusCode: response.status,
            message: response.data.message
        }
    } catch (error: any) {
        return errorHandler(error);
    }
}
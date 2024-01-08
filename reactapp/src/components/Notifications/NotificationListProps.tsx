interface NotificationListProps {
    notifications: NotificationProps[] | null,
    deleteNotification?: (notificationId: number) => void,
    error?: string
}
using webapi.Models;

namespace webapi.Interfaces.SQL.Notifications
{
    public interface IReadNotifications
    {
        Task<NotificationModel> ReadNotificationById(int id);
        Task<List<NotificationModel>> ReadAllSendedNotifications(int id);
        Task<List<NotificationModel>> ReadAllReceivedNotifications(int id);
    }
}

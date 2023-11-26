using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL.Notifications;
using webapi.Localization.English;
using webapi.Models;

namespace webapi.DB.SQL.Notifications
{
    public class ReadNotifications : IReadNotifications
    {
        private readonly FileCryptDbContext _dbContext;

        public ReadNotifications(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NotificationModel> ReadNotificationById(int id)
        {
            var notification = await _dbContext.Notifications.FirstOrDefaultAsync(u => u.notification_id == id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);

            return notification;
        }

        public async Task<List<NotificationModel>> ReadAllSendedNotifications(int id)
        {
            var notifications = await _dbContext.Notifications.Where(n => n.sender_id == id).ToListAsync() ??
                throw new NotificationException(ExceptionNotificationMessages.NoOneNotificationNotFound);

            return notifications;
        }

        public async Task<List<NotificationModel>> ReadAllReceivedNotifications(int id)
        {
            var notifications = await _dbContext.Notifications.Where(n => n.receiver_id == id).ToListAsync() ??
                throw new NotificationException(ExceptionNotificationMessages.NoOneNotificationNotFound);

            return notifications;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using webapi.Exceptions;
using webapi.Interfaces.SQL;
using webapi.Localization;
using webapi.Localization.Exceptions;
using webapi.Models;

namespace webapi.DB.SQL
{
    public class Notifications : ICreate<NotificationModel>, IDelete<NotificationModel>, IRead<NotificationModel>
    {
        private readonly FileCryptDbContext _dbContext;

        public Notifications(FileCryptDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Create(NotificationModel notificationModel)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == notificationModel.receiver_id);

            if (user is null)
                throw new UserException(AccountErrorMessage.UserNotFound);

            await _dbContext.AddAsync(notificationModel);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<NotificationModel> ReadById(int id, bool? byForeign)
        {
            var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.notification_id == id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);

            notification.is_checked = true;
            await _dbContext.SaveChangesAsync();

            return notification;
        }

        public async Task<IEnumerable<NotificationModel>> ReadAll(int? user_id, int skip, int count)
        {
            var query = _dbContext.Notifications
                .OrderByDescending(n => n.send_time)
                .AsQueryable();

            if (user_id.HasValue)
                return await query.Where(n => n.receiver_id == user_id).Skip(skip).Take(count).ToListAsync();

            return await query.Skip(skip).Take(count).ToListAsync();
        }

        public async Task DeleteById(int id, int? user_id)
        {
            var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.notification_id == id && n.receiver_id == user_id) ??
                throw new NotificationException(ExceptionNotificationMessages.NotificationNotFound);

            if (notification.priority != Priority.Info.ToString())
                throw new NotificationException(ExceptionNotificationMessages.CannotDelete);

            _dbContext.Notifications.Remove(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}
